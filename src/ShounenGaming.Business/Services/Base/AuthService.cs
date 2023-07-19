using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.DataAccess.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ShounenGaming.Core.Entities.Base;
using ShounenGaming.Business.Hubs;
using Microsoft.AspNetCore.SignalR;
using ShounenGaming.DTOs.Models.Base;
using ShounenGaming.Core.Entities.Base.Enums;
using AutoMapper;

namespace ShounenGaming.Business.Services.Base
{
    // TODO: Rethink the creation, since I can create and not verify on a other person's account (maybe change or save in cache)
    public class AuthService : IAuthService
    {
        private readonly AppSettings _appSettings;
        private readonly IMemoryCache _cache;
        private readonly IUserRepository _userRepo;
        private readonly IServerMemberRepository _memberRepo;
        private readonly IHubContext<DiscordEventsHub, IDiscordEventsHubClient> _authHub;

        private readonly IMapper _mapper;

        public AuthService(AppSettings appSettings, IMemoryCache cache, IUserRepository userRepo, IHubContext<DiscordEventsHub, IDiscordEventsHubClient> authHub, IServerMemberRepository memberRepo, IMapper mapper)
        {
            _cache = cache;
            _userRepo = userRepo;
            _authHub = authHub;
            _memberRepo = memberRepo;
            _appSettings = appSettings;
            _mapper = mapper;
        }

        public async Task<List<ServerMemberDTO>> GetNotRegisteredServerMembers()
        {
            var members = await _memberRepo.GetUnregisteredServerMembers();
            return _mapper.Map<List<ServerMemberDTO>>(members);
        }


        public async Task RegisterUser(CreateUser createUser)
        {
            //Validate DiscordId
            var serverMember = await _memberRepo.GetMemberByDiscordId(createUser.DiscordId) ?? throw new EntityNotFoundException("ServerMember");

            //Validate Date
            if (createUser.Birthday.Year > DateTime.UtcNow.Year - 5 || createUser.Birthday.Year < DateTime.UtcNow.Year - 100)
                throw new InvalidParameterException("Birthday", $"Needs to be between {DateTime.UtcNow.Year - 100} and {DateTime.UtcNow.Year - 5}");

            if (serverMember.User is not null)
            {
                serverMember.User.FirstName = createUser.FirstName;
                serverMember.User.LastName = createUser.LastName;
                serverMember.User.DiscordVerified = false;
                serverMember.User.Username = string.IsNullOrEmpty(createUser.Username) ? serverMember.Username : createUser.Username;
                serverMember.User.Birthday = new DateTime(createUser.Birthday.Year, createUser.Birthday.Month, createUser.Birthday.Day, 0, 0, 0, DateTimeKind.Utc);
                await _userRepo.Update(serverMember.User);
            } 
            else
            {
                await _userRepo.Create(new User
                {
                    FirstName = createUser.FirstName,
                    LastName = createUser.LastName,
                    Username = string.IsNullOrEmpty(createUser.Username) ? serverMember.Username : createUser.Username,
                    Birthday = new DateTime(createUser.Birthday.Year, createUser.Birthday.Month, createUser.Birthday.Day, 0, 0, 0, DateTimeKind.Utc),
                    DiscordVerified = false,
                    ServerMember = serverMember,
                    MangasConfigurations = new UserMangasConfigurations()
                });

            }

            await _authHub.Clients.All.SendVerifyAccount(createUser.DiscordId, createUser.FirstName + " " + createUser.LastName, createUser.Birthday);
        }

        public async Task RequestEntryToken(string username)
        {
            var user = await _userRepo.GetUserByUsername(username) ?? throw new EntityNotFoundException("User");

            if (!user.IsInServer)
                throw new InvalidOperationException("You're not on the Server");

            if (!user.DiscordVerified)
                throw new DiscordAccountNotConfirmedException();
            
            var dataHolder = new CacheHolder
            {
                Token = GenerateRandomString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            };

            _cache.Set(username, dataHolder, dataHolder.ExpiresAt);
            await _authHub.Clients.All.SendToken(user.ServerMember.DiscordId, dataHolder.Token, dataHolder.ExpiresAt);

            Log.Information($"User {user.FirstName} {user.LastName} created token {dataHolder.Token}");
        }

        public async Task<AuthResponse> LoginUser(string username, string token)
        {
            var existsEntry = _cache.TryGetValue(username, out CacheHolder dataHolder);

            if (!existsEntry)
                throw new EntityNotFoundException("Token");

            if (token != dataHolder.Token)
                throw new WrongCredentialsException("Token");


            var user = await _userRepo.GetUserByUsername(username) ?? throw new EntityNotFoundException("User");

            _cache.Remove(username);

            user.RefreshToken = await GenerateRefreshToken();
            user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(7);

            var updatedUser = await _userRepo.Update(user);
            Log.Information($"User {updatedUser.FirstName} {updatedUser.LastName} logged in");

            return GetAuthResponseForUser(updatedUser);
        }

        public AuthResponse LoginBot(string discordId, string password)
        {
            var botSettings = _appSettings.DiscordBot;
            if (discordId != botSettings.DiscordId || password != botSettings.Password)
                throw new WrongCredentialsException("Wrong Credentials");

            Log.Information($"Bot {discordId} logged in");
            return new AuthResponse
            {
                RefreshToken = string.Empty,
                AccessToken = GenerateAccessToken(new List<Claim>
                    {
                        new Claim("DiscordId", discordId),
                        new Claim("Role", "Bot"),
                    }, DateTime.UtcNow.AddMinutes(1)),
            }; 
        }
        public async Task<AuthResponse> RefreshToken(string refreshToken)
        {
            var user = await _userRepo.GetUserByRefreshToken(refreshToken) ?? throw new EntityNotFoundException("RefreshToken");

            if (user.RefreshTokenExpiryDate < DateTime.UtcNow)
            {
                user.RefreshToken = null;
                await _userRepo.Update(user);

                throw new InvalidOperationException("RefreshToken expired");
            }

            //Handle RefreshToken if less than 3 days to expire
            if (user.RefreshTokenExpiryDate!.Value.AddDays(-3) <= DateTime.UtcNow)
            {
                user.RefreshToken = await GenerateRefreshToken();
                user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(7);
            }

            var updatedUser = await _userRepo.Update(user);
            return GetAuthResponseForUser(updatedUser);
        }
       
        public async Task AcceptAccountVerification(string discordId)
        {
            var user = await _userRepo.GetUserByDiscordId(discordId) ?? throw new EntityNotFoundException("User");           
            user.DiscordVerified = true;
            await _userRepo.Update(user);
        }
        public async Task RejectAccountVerification(string discordId)
        {
            var user = await _userRepo.GetUserByDiscordId(discordId) ?? throw new EntityNotFoundException("User");
            await _userRepo.Delete(user.Id);
        }

        #region Private
        private async Task<string> GenerateRefreshToken()
        {
            bool newToken;
            string refreshToken;
            do
            {
                var randomNumber = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                    refreshToken = Convert.ToBase64String(randomNumber);
                }
                newToken = (await _userRepo.GetUserByRefreshToken(refreshToken)) == null;
            } while (!newToken);

            return refreshToken;
        }
        
        private AuthResponse GetAuthResponseForUser(User user)
        {
            Log.Information($"{user!.FirstName} {user!.LastName} generated tokens");
            return new AuthResponse
            {
                RefreshToken = user.RefreshToken!,
                AccessToken = GenerateAccessToken(new List<Claim>
                    {
                        new Claim("Id", user.Id.ToString()),
                        new Claim("Username", user.Username),
                        new Claim("DiscordId", user.ServerMember!.DiscordId),
                        new Claim("Role", user.ServerMember.Role.ToString()),
                    }, DateTime.UtcNow.AddMinutes(60)),
            };
        }
        private static string HashPassword(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        }

        private static byte[] GenerateSalt()
        {
            return RandomNumberGenerator.GetBytes(128/8);
        }

        private string GenerateAccessToken(List<Claim> claims, DateTime expiresAt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_appSettings.JwtSecret); 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private static string GenerateRandomString()
        {
            var allChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var resultToken = new string(
               Enumerable.Repeat(allChar, 8)
               .Select(token => token[random.Next(token.Length)]).ToArray());

            return resultToken.ToString();
        }

        public async Task UpdateServerMember(string discordId, string discordImageUrl, string displayName, string username, RolesEnum? role)
        {
            var member = await _memberRepo.GetMemberByDiscordId(discordId);
            if (member is null && role.HasValue)
            {
                // Create
                await _memberRepo.Create(new ServerMember
                {
                    DiscordId = discordId,
                    ImageUrl = discordImageUrl,
                    DisplayName = displayName,
                    Username = username,
                    Role = role.Value
                });
            } 
            else if (member is not null)
            {
                // Update
                if (role.HasValue)
                {
                    member.Role = role.Value;
                    member.DisplayName = displayName;
                    member.Username = username;
                    member.ImageUrl = discordImageUrl;

                    await _memberRepo.Update(member);
                } 
                // Remove
                else
                {
                    await _memberRepo.Delete(member.Id);

                    var user = await _userRepo.GetUserByDiscordId(discordId);
                    if (user is null) return;

                    user.DiscordVerified = false;
                    await _userRepo.Update(user);
                }
            }
        }

        

        private class CacheHolder
        {
            public string Token { get; set; }
            public DateTime ExpiresAt { get; set; }
        }

        #endregion
    }

}


