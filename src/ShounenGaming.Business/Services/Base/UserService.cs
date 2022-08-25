using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Business.Models.Base;
using ShounenGaming.DataAccess.Interfaces.Base;

namespace ShounenGaming.Business.Services.Base
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<List<UserDTO>> GetUsers()
        {
            var users = await _userRepo.GetAll();
            return users.Select(u => new UserDTO
            {
                Id = u.Id,
                FullName = u.FirstName + u.LastName,
                Birthday = u.Birthday,
                DiscordId = u.DiscordId,
            }).ToList();
        }
    }
}
