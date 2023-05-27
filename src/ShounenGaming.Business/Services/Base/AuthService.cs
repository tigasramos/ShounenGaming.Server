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

namespace ShounenGaming.Business.Services.Base
{
    public class AuthService : IAuthService
    {
        private readonly IMemoryCache _cache;
        private readonly IUserRepository _userRepo;
        private readonly IBotRepository _botRepo;
        private readonly IHubContext<AuthHub, IAuthHubClient> _authHub;

        public AuthService(IMemoryCache cache, IUserRepository userRepo, IBotRepository botRepo, IHubContext<AuthHub, IAuthHubClient> authHub)
        {
            _cache = cache;
            _userRepo = userRepo;
            _botRepo = botRepo;
            _authHub = authHub;
        }

        public async Task RegisterBot(CreateBot createBot)
        {
            var salt = GenerateSalt();
            await _botRepo.Create(new Bot
            {
                DiscordId = createBot.DiscordId,
                Salt = Convert.ToBase64String(salt),
                PasswordHashed = HashPassword(createBot.Password, salt),
            });
        }
        public async Task RegisterUser(CreateUser createUser)
        {
            //Validate Date
            if (createUser.Birthday.Year > DateTime.UtcNow.Year - 5 || createUser.Birthday.Year < DateTime.UtcNow.Year - 100)
                throw new InvalidParameterException("Birthday", $"Needs to be between {DateTime.UtcNow.Year - 100} and {DateTime.UtcNow.Year - 5}");

            //Validate DiscordId
            //var unregisteredUsers = await GetUnregisteredUsers();
            //if (!unregisteredUsers.Any(u => createUser.DiscordId == u.DiscordId))
            //    throw new InvalidParameterException("DiscordId", "Discord Id was not found or already has an account");

            await _userRepo.Create(new User
            {
                FirstName = createUser.FirstName,
                LastName = createUser.LastName,
                Username = createUser.Username,
                Birthday = new DateTime(createUser.Birthday.Year, createUser.Birthday.Month, createUser.Birthday.Day,0,0,0, DateTimeKind.Utc),
                DiscordId = createUser.DiscordId,
                DiscordVerified = false,
                Email = createUser.Email,
                EmailVerified = false,
                Role = Core.Entities.Base.Enums.RolesEnum.USER
            });

            await _authHub.Clients.All.SendVerifyAccount(createUser.DiscordId, createUser.FirstName + createUser.LastName);
        }
        public async Task RequestEntryToken(string username)
        {
            var user = await _userRepo.GetUserByUsername(username);
            if (user == null)
                throw new EntityNotFoundException("User");

            //TODO: Remove after testing
            //if (!user.DiscordAccountConfirmed)
            //    throw new DiscordAccountNotConfirmedException();
            
            var dataHolder = new CacheHolder
            {
                Token = GenerateRandomString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            };

            _cache.Set(username, dataHolder, dataHolder.ExpiresAt);
            await _authHub.Clients.All.SendToken(user.DiscordId, dataHolder.Token, dataHolder.ExpiresAt);

            Log.Information($"User {user.FirstName} {user.LastName} created token {dataHolder.Token}");
        }
        public async Task<AuthResponse> LoginUser(string username, string token)
        {
            var existsEntry = _cache.TryGetValue(username, out CacheHolder dataHolder);

            if (!existsEntry)
                throw new EntityNotFoundException("Token");

            if (token != dataHolder.Token)
                throw new WrongCredentialsException("Token");


            var user = await _userRepo.GetUserByUsername(username);
            if (user == null)
                throw new EntityNotFoundException("User");

            _cache.Remove(username);

            user.RefreshToken = await GenerateRefreshToken();
            user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(7);

            var updatedUser = await _userRepo.Update(user);
            Log.Information($"User {updatedUser.FirstName} {updatedUser.LastName} logged in");

            return GetAuthResponse(updatedUser);
        }
        public async Task<AuthResponse> LoginBot(string discordId, string password)
        {
            var bot = await _botRepo.GetBotByDiscordId(discordId);
            if (bot == null)
                throw new EntityNotFoundException("Bot");

            //Verify Password
            bool samePassword = HashPassword(password, Convert.FromBase64String(bot.Salt)) == bot.PasswordHashed;

            if (!samePassword)
                throw new WrongCredentialsException("Password");

            //Handle RefreshToken
            bot.RefreshToken = await GenerateRefreshToken(true);
            bot.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(7);

            var updatedBot = await _botRepo.Update(bot);
            Log.Information($"Bot {updatedBot.DiscordId} logged in");

            return GetAuthResponse(updatedBot);
        }
        public async Task<AuthResponse> RefreshToken(string refreshToken, bool isBot)
        {
            AuthEntity? entity = isBot ? await _botRepo.GetBotByRefreshToken(refreshToken) : await _userRepo.GetUserByRefreshToken(refreshToken);
            if (entity == null)
                throw new EntityNotFoundException("RefreshToken");

            if (entity.RefreshTokenExpiryDate < DateTime.UtcNow)
            {
                entity.RefreshToken = null;
                if (isBot)
                    await _botRepo.Update(entity as Bot);
                else
                    await _userRepo.Update(entity as User);

                throw new InvalidOperationException("RefreshToken expired");
            }

            AuthEntity updatedEntity = entity;

            //Handle RefreshToken if less than 3 days to expire
            if (entity.RefreshTokenExpiryDate!.Value.AddDays(3) > DateTime.UtcNow)
            {
                entity.RefreshToken = await GenerateRefreshToken(isBot);
                entity.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(7);

                if (isBot)
                    updatedEntity = await _botRepo.Update(entity as Bot);
                else
                    updatedEntity = await _userRepo.Update(entity as User);
            }
           
            return GetAuthResponse(updatedEntity);
        }
        public async Task<List<DiscordUserDTO>> GetUnregisteredUsers()
        {
            //Get Discord Users
            var usersFound = _cache.TryGetValue("DiscordUsers", out List<DiscordUserDTO> allUsers);
            if (!usersFound)
                return new List<DiscordUserDTO>();

            //Get Users
            var registeredUsers = await _userRepo.GetAll();

            return allUsers.Where(u => !registeredUsers.Any(ru => ru.DiscordId == u.DiscordId)).ToList();
        }
        public async Task VerifyDiscordAccount(string discordId)
        {
            var user = await _userRepo.GetUserByDiscordId(discordId);
            if (user == null)
                throw new EntityNotFoundException("User");

            user.DiscordVerified = true;
            await _userRepo.Update(user);
        }
        public void SetDiscordUsers(List<DiscordUserDTO> users)
        {
            _cache.Set("DiscordUsers", users);
        }


        #region Private
        private async Task<string> GenerateRefreshToken(bool isBot = false)
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
                newToken = isBot ? (await _userRepo.GetUserByRefreshToken(refreshToken)) == null : (await _userRepo.GetUserByRefreshToken(refreshToken)) == null;
            } while (!newToken);

            return refreshToken;
        }
        private AuthResponse GetAuthResponse(AuthEntity entity)
        {
            if (entity is Bot)
            {
                var bot = entity as Bot;
                Log.Information($"{bot.DiscordId} generated tokens");
                return new AuthResponse
                {
                    RefreshToken = bot.RefreshToken!,
                    AccessToken = GenerateAccessToken(new List<Claim>
                    {
                        new Claim("Id", bot.Id.ToString()),
                        new Claim("DiscordId", bot.DiscordId),
                        new Claim("Role", "Bot"),
                    }, DateTime.UtcNow.AddMinutes(60)),
                };
            }
            else
            {
                var user = entity as User;
                Log.Information($"{user!.FirstName} {user!.LastName} generated tokens");
                return new AuthResponse
                {
                    RefreshToken = user.RefreshToken!,
                    AccessToken = GenerateAccessToken(new List<Claim>
                    {
                        new Claim("Id", user.Id.ToString()),
                        new Claim("Username", user.Username),
                        new Claim("DiscordId", user.DiscordId),
                        new Claim("Role", user.Role.ToString()),
                    }, DateTime.UtcNow.AddMinutes(5)),
                };
            }

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
        private static string GenerateAccessToken(List<Claim> claims, DateTime expiresAt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            //TODO: Get right configuration
            var key = Encoding.ASCII.GetBytes("TODO_SUPER_HUGE_SECRET_KEY"); 
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

       
        private class CacheHolder
        {
            public string Token { get; set; }
            public DateTime ExpiresAt { get; set; }
        }

        #endregion
    }

}


