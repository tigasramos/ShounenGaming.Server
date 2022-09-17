using AutoMapper;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Interfaces.Tierlists;
using ShounenGaming.Business.Models.Tierlists;
using ShounenGaming.Business.Models.Tierlists.Requests;
using ShounenGaming.DataAccess.Interfaces.Base;
using ShounenGaming.DataAccess.Interfaces.Tierlists;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Services.Tierlists
{
    public class TierlistService : ITierlistService
    {
        private readonly ITierlistRepository _tierlistRepo;
        private readonly IUserTierlistRepository _userTierlistRepo;
        private readonly ITierlistCategoryRepository _tierlistCategoryRepo;
        private readonly IFileDataRepository _fileRepo;

        private readonly IMapper _mapper;

        public TierlistService(ITierlistRepository tierlistRepo, IUserTierlistRepository userTierlistRepo, ITierlistCategoryRepository tierlistCategoryRepo, IMapper mapper, IFileDataRepository fileRepo)
        {
            _tierlistRepo = tierlistRepo;
            _userTierlistRepo = userTierlistRepo;
            _tierlistCategoryRepo = tierlistCategoryRepo;
            _mapper = mapper;
            _fileRepo = fileRepo;
        }

        //Tierlist
        public async Task<TierlistDTO> GetTierlistById(int id)
        {
            var tierlist = await _tierlistRepo.GetById(id);
            if (tierlist == null)
                throw new EntityNotFoundException("Tierlist");

            return _mapper.Map<TierlistDTO>(tierlist);
        }
        public async Task<TierlistDTO> CreateTierlist(CreateTierlist createTierlist, int userId)
        {
            //Verify Category
            var tierlistCategory = await _tierlistCategoryRepo.GetById(createTierlist.CategoryId);
            if (tierlistCategory == null)
                throw new EntityNotFoundException("TierlistCategory");

            //Verify Picture
            var image = await _fileRepo.GetById(createTierlist.ImageId);
            if (tierlistCategory == null)
                throw new EntityNotFoundException("Image");

            //Verify Orders
            if (createTierlist.DefaultTiers.Select(t => t.Order).Distinct().Count() != createTierlist.DefaultTiers.Count)
                throw new Exception("Error with Default Tiers Orders");

            var tierlist = await _tierlistRepo.Create(new Core.Entities.Tierlists.Tierlist
            {
                UserId = userId,
                Category = tierlistCategory,
                Name = createTierlist.Name,
                ImageId = createTierlist.ImageId,
                DefaultTiers = createTierlist.DefaultTiers.Select(t => new Core.Entities.Tierlists.Tier{ Name = t.Name, Order = t.Order, ColorHex = t.ColorHex }).ToList()
            });

            return _mapper.Map<TierlistDTO>(tierlist);
        }
        public async Task<TierlistDTO> EditTierlist(EditTierlist editTierlist, int userId)
        {
            var tierlist = await _tierlistRepo.GetById(editTierlist.Id);
            if (tierlist == null)
                throw new EntityNotFoundException("Tierlist");

            if (tierlist.UserId != userId)
                throw new NoPermissionException();

            //Verify Image
            if (editTierlist.ImageId != null)
            {
                var image = await _fileRepo.GetById(editTierlist.ImageId.Value);
                if (image == null)
                    throw new EntityNotFoundException("Image");
            } 

            tierlist.CategoryId = editTierlist.CategoryId ?? tierlist.CategoryId;
            tierlist.Name = editTierlist.Name ?? tierlist.Name;
            tierlist.ImageId = editTierlist.ImageId ?? tierlist.ImageId;

            tierlist = await _tierlistRepo.Update(tierlist);
            return _mapper.Map<TierlistDTO>(tierlist);
        }
        public async Task<TierlistDTO> AddDefaultTier(int tierlistId, CreateTier tier, int userId)
        {
            var tierlist = await _tierlistRepo.GetById(tierlistId);
            if (tierlist == null)
                throw new EntityNotFoundException("Tierlist");

            if (tierlist.UserId != userId)
                throw new NoPermissionException();

            if (tierlist.DefaultTiers == null)
                tierlist.DefaultTiers = new();

            //Verify Order
            if (tierlist.DefaultTiers.Any(dt => dt.Order == tier.Order)) 
            { 
                foreach(var dtier in tierlist.DefaultTiers)
                {
                    if (dtier.Order >= tier.Order)
                    {
                        dtier.Order++;
                    }
                }
            }

            tierlist.DefaultTiers.Add(new Core.Entities.Tierlists.Tier{
                Name = tier.Name,
                ColorHex = tier.ColorHex,
                Order = tier.Order,
            });

            tierlist = await _tierlistRepo.Update(tierlist);

            return _mapper.Map<TierlistDTO>(tierlist);
        }
        public async Task<TierlistDTO> EditDefaultTier(int tierlistId, EditTier newTier, int userId)
        {
            var tierlist = await _tierlistRepo.GetById(tierlistId);
            if (tierlist == null)
                throw new EntityNotFoundException("Tierlist");

            if (tierlist.UserId != userId)
                throw new NoPermissionException();

            var oldTier = tierlist.DefaultTiers.SingleOrDefault(dt => dt.Id == newTier.Id);
            if (oldTier == null)
                throw new EntityNotFoundException("Tier");

            //Verify Order
            if (tierlist.DefaultTiers.Any(dt => dt.Order == newTier.Order))
            {
                if (oldTier.Order > newTier.Order)
                {
                    //Lowered number
                    foreach(var dt in tierlist.DefaultTiers)
                    {
                        if (dt.Order >= newTier.Order && dt.Order < oldTier.Order)
                        {
                            dt.Order++;
                        }
                    }
                }
                else if (oldTier.Order < newTier.Order)
                {
                    //Raised number
                    foreach (var dt in tierlist.DefaultTiers)
                    {
                        if (dt.Order <= newTier.Order && dt.Order > oldTier.Order)
                        {
                            dt.Order--;
                        }
                    }
                }
            }

            oldTier.Name = newTier.Name ?? oldTier.Name;
            oldTier.ColorHex = newTier.ColorHex ?? oldTier.ColorHex;
            oldTier.Order = newTier.Order ?? oldTier.Order;

            tierlist = await _tierlistRepo.Update(tierlist);
            return _mapper.Map<TierlistDTO>(tierlist);
        }
        public async Task<TierlistDTO> RemoveDefaultTier(int tierlistId, int tierId, int userId)
        {
            var tierlist = await _tierlistRepo.GetById(tierlistId);
            if (tierlist == null)
                throw new EntityNotFoundException("Tierlist");

            if (tierlist.UserId != userId)
                throw new NoPermissionException();

            var tier = tierlist.DefaultTiers.SingleOrDefault(dt => dt.Id == tierId);
            if (tier == null)
                throw new EntityNotFoundException("Tier");

            foreach(var dt in tierlist.DefaultTiers)
            {
                if (dt.Order > tier.Order)
                {
                    dt.Order--;
                }
            }

            //TODO: Test if deletes from Tier table or not
            tierlist.DefaultTiers.Remove(tier);
            tierlist = await _tierlistRepo.Update(tierlist);
            return _mapper.Map<TierlistDTO>(tierlist);
        }
        public async Task<TierlistDTO> AddTierlistItem(int tierlistId, CreateTierlistItem tierlistItem, int userId)
        {
            var tierlist = await _tierlistRepo.GetById(tierlistId);
            if (tierlist == null)
                throw new EntityNotFoundException("Tierlist");

            if (tierlist.UserId != userId)
                throw new NoPermissionException();

            var image = await _fileRepo.GetById(tierlistItem.ImageId);
            if (image == null)
                throw new EntityNotFoundException("Image");
            
            if (tierlist.Items == null)
                tierlist.Items = new();

            tierlist.Items.Add(new Core.Entities.Tierlists.TierlistItem
            {
                Image = image,
                Name = tierlist.Name,
            });

            tierlist = await _tierlistRepo.Update(tierlist);
            return _mapper.Map<TierlistDTO>(tierlist);
        }


        //User
        public async Task<UserTierlistDTO> CreateUserTierlist(CreateUserTierlist createUserTierlist, int userId)
        {
            var tierlist = await _tierlistRepo.GetById(createUserTierlist.TierlistId);
            if (tierlist == null)
                throw new EntityNotFoundException("Tierlist");

            var userTierlist = await _userTierlistRepo.Create(new Core.Entities.Tierlists.UserTierlist
            {
                TierlistId = createUserTierlist.TierlistId,
                Name = createUserTierlist.Name,
                UserId = userId
            });

            return _mapper.Map<UserTierlistDTO>(userTierlist);
        }
        public async Task<UserTierlistDTO> AddTierToUserTierlist(int userTierlistId, CreateTier tier, int userId)
        {
            var userTierlist = await _userTierlistRepo.GetById(userTierlistId);
            if (userTierlist == null)
                throw new EntityNotFoundException("UserTierlist");

            if (userTierlist.UserId != userId)
                throw new NoPermissionException();

            //Verify Order
            if (userTierlist.Choices.Any(dt => dt.Tier.Order == tier.Order))
            {
                foreach (var dtier in userTierlist.Choices)
                {
                    if (dtier.Tier.Order >= tier.Order)
                    {
                        dtier.Tier.Order++;
                    }
                }
            }

            userTierlist.Choices.Add(new Core.Entities.Tierlists.TierChoice
            {
                Tier = new Core.Entities.Tierlists.Tier
                {
                    ColorHex = tier.ColorHex,
                    Name = tier.Name,
                    Order = tier.Order,
                }
            });

            userTierlist = await _userTierlistRepo.Update(userTierlist);
            return _mapper.Map<UserTierlistDTO>(userTierlist);
        }
        public async Task<UserTierlistDTO> EditTierFromUserTierlist(int userTierlistId, EditTier newTier, int userId)
        {
            var userTierlist = await _userTierlistRepo.GetById(userTierlistId);
            if (userTierlist == null)
                throw new EntityNotFoundException("UserTierlist");

            if (userTierlist.UserId != userId)
                throw new NoPermissionException();

            var oldTier = userTierlist.Choices.SingleOrDefault(t => t.Tier.Id == newTier.Id);
            if (oldTier == null)
                throw new EntityNotFoundException("Tier");

            //Verify Order
            if (userTierlist.Choices.Any(dt => dt.Tier.Order == newTier.Order))
            {
                if (oldTier.Tier.Order > newTier.Order)
                {
                    //Lowered number
                    foreach (var dt in userTierlist.Choices)
                    {
                        if (dt.Tier.Order >= newTier.Order && dt.Tier.Order < oldTier.Tier.Order)
                        {
                            dt.Tier.Order++;
                        }
                    }
                }
                else if (oldTier.Tier.Order < newTier.Order)
                {
                    //Raised number
                    foreach (var dt in userTierlist.Choices)
                    {
                        if (dt.Tier.Order <= newTier.Order && dt.Tier.Order > oldTier.Tier.Order)
                        {
                            dt.Tier.Order--;
                        }
                    }
                }
            }

            oldTier.Tier.Order = newTier.Order ?? oldTier.Tier.Order;
            oldTier.Tier.Name = newTier.Name ?? oldTier.Tier.Name;
            oldTier.Tier.ColorHex = newTier.ColorHex ?? oldTier.Tier.ColorHex;

            userTierlist = await _userTierlistRepo.Update(userTierlist);
            return _mapper.Map<UserTierlistDTO>(userTierlist);
        }
        public async Task<UserTierlistDTO> RemoveTierFromUserTierlist(int userTierlistId, int tierId, int userId)
        {
            var userTierlist = await _userTierlistRepo.GetById(userTierlistId);
            if (userTierlist == null)
                throw new EntityNotFoundException("UserTierlist");

            if (userTierlist.UserId != userId)
                throw new NoPermissionException();

            userTierlist.Choices.RemoveAll(c => c.Tier.Id == tierId);
            userTierlist = await _userTierlistRepo.Update(userTierlist);
            return _mapper.Map<UserTierlistDTO>(userTierlist);
        }
        public async Task<UserTierlistDTO> EditUserTierlist(EditUserTierlist editUserTierlist, int userId)
        {
            var userTierlist = await _userTierlistRepo.GetById(editUserTierlist.Id);
            if (userTierlist == null)
                throw new EntityNotFoundException("UserTierlist");

            if (userTierlist.UserId != userId)
                throw new NoPermissionException();

            userTierlist.Name = editUserTierlist.Name ?? userTierlist.Name;

            var tierlist = userTierlist.Tierlist;

            foreach(var tierChoice in editUserTierlist.Choices)
            {
                var dbTier = userTierlist.Choices.SingleOrDefault(c => c.Tier.Id == tierChoice.TierId);
                if (dbTier == null) continue;

                dbTier.Items.Clear();
                foreach (var itemId in tierChoice.ItemsIds)
                {
                    var item = tierlist.Items.SingleOrDefault(i => i.Id == itemId);
                    if (item == null) continue;

                    dbTier.Items.Add(item);
                }
            }

            userTierlist = await _userTierlistRepo.Update(userTierlist);
            return _mapper.Map<UserTierlistDTO>(userTierlist);
        }
        public async Task<List<UserTierlistDTO>> GetUserTierlists()
        {
            var userTierlists = await _userTierlistRepo.GetAll();
            return _mapper.Map<List<UserTierlistDTO>>(userTierlists);
        }
        public async Task<List<UserTierlistDTO>> GetUserTierlistsByUserId(int userId)
        {
            var userTierlists = await _userTierlistRepo.GetUserTierlistsByUserId(userId);
            return _mapper.Map<List<UserTierlistDTO>>(userTierlists);
        }
        public async Task<List<UserTierlistDTO>> GetUserTierlistsByTierlistId(int tierlistId)
        {
            var userTierlists = await _userTierlistRepo.GetUserTierlistsByTierlistId(tierlistId);
            return _mapper.Map<List<UserTierlistDTO>>(userTierlists);
        }


        //Category
        public async Task<List<TierlistCategoryDTO>> GetTierlistCategories()
        {
            var categories = await _tierlistCategoryRepo.GetAll();
            return _mapper.Map<List<TierlistCategoryDTO>>(categories);
        }
        public async Task<TierlistCategoryDTO> CreateTierlistCategory(CreateTierlistCategory tierListCategory)
        {
            //Verify Image
            var image = await _fileRepo.GetById(tierListCategory.ImageId);
            if (image == null)
                throw new EntityNotFoundException("Image");

            var category = await _tierlistCategoryRepo.Create(new Core.Entities.Tierlists.TierlistCategory
            {
                Description = tierListCategory.Description,
                Name = tierListCategory.Name,
                ImageId = tierListCategory.ImageId,
            });

            return _mapper.Map<TierlistCategoryDTO>(category);

        }
    }
}
