using AutoMapper;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Business.Models.Base;
using ShounenGaming.DataAccess.Interfaces.Base;

namespace ShounenGaming.Business.Services.Base
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<UserDTO> GetUserById(int id)
        {
            var user = await _userRepo.GetById(id);
            if (user == null)
                throw new EntityNotFoundException("User");
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<List<UserDTO>> GetUsers()
        {
            var users = await _userRepo.GetAll();
            return _mapper.Map<List<UserDTO>>(users);
        }
    }
}
