using ShounenGaming.DTOs.Models.Base;

namespace ShounenGaming.Business.Interfaces.Base
{
    public interface IUserService
    {
        Task<UserMangasConfigsDTO> GetUserConfigsForMangas(int userId);
        Task<UserMangasConfigsDTO> ChangeUserConfigsForMangas(int userId, ChangeUserMangasConfigsDTO updateConfigs);
        Task<UserDTO> GetUserById(int id);
        Task<List<UserDTO>> GetUsers();
    }
}
