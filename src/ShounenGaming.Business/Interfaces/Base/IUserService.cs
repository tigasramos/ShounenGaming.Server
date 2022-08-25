using ShounenGaming.Business.Models.Base;

namespace ShounenGaming.Business.Interfaces.Base
{
    public interface IUserService
    {
        Task<List<UserDTO>> GetUsers();
    }
}
