using ShounenGaming.Business.Models.Tierlists;
using ShounenGaming.Business.Models.Tierlists.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Interfaces.Tierlists
{
    public interface ITierlistService
    {
        //Tierlist
        Task<TierlistDTO> GetTierlistById(int id);
        Task<TierlistDTO> CreateTierlist(CreateTierlist createTierlist, int userId);
        Task<TierlistDTO> EditTierlist(EditTierlist editTierlist, int userId);
        Task<TierlistDTO> AddDefaultTier(int tierlistId, CreateTier tier, int userId); 
        Task<TierlistDTO> EditDefaultTier(int tierlistId, EditTier tier, int userId); 
        Task<TierlistDTO> RemoveDefaultTier(int tierlistId, int tierId, int userId);
        Task<TierlistDTO> AddTierlistItem(int tierlistId, CreateTierlistItem tierlistItem, int userId);


        //User
        Task<UserTierlistDTO> CreateUserTierlist(CreateUserTierlist userTierlist, int userId);
        Task<UserTierlistDTO> AddTierToUserTierlist(int userTierlistId, CreateTier tier, int userId);
        Task<UserTierlistDTO> EditTierFromUserTierlist(int userTierlistId, EditTier tier, int userId);
        Task<UserTierlistDTO> RemoveTierFromUserTierlist(int userTierlistId, int tierId, int userId);
        Task<UserTierlistDTO> EditUserTierlist(EditUserTierlist userTierlist, int userId);
        Task<List<UserTierlistDTO>> GetUserTierlists();
        Task<List<UserTierlistDTO>> GetUserTierlistsByUserId(int userId);
        Task<List<UserTierlistDTO>> GetUserTierlistsByTierlistId(int tierlistId);


        //Category
        Task<List<TierlistCategoryDTO>> GetTierlistCategories();
        Task<TierlistCategoryDTO> CreateTierlistCategory(CreateTierlistCategory tierListCategory);

    }
}
