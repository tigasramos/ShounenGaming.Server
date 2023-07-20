using AutoMapper;
using ShounenGaming.Business.Exceptions;
using ShounenGaming.Business.Interfaces.Base;
using ShounenGaming.Core.Entities.Mangas.Enums;
using ShounenGaming.DataAccess.Interfaces.Base;
using ShounenGaming.DTOs.Models.Base;

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
            var user = await _userRepo.GetById(id) ?? throw new EntityNotFoundException("User");
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserMangasConfigsDTO> GetUserConfigsForMangas(int userId)
        {
            var user = await _userRepo.GetById(userId) ?? throw new EntityNotFoundException("User");
            return _mapper.Map<UserMangasConfigsDTO>(user.MangasConfigurations);
        }

        public async Task<UserMangasConfigsDTO> ChangeUserConfigsForMangas(int userId, ChangeUserMangasConfigsDTO updateConfigs)
        {
            var user = await _userRepo.GetById(userId) ?? throw new EntityNotFoundException("User");

            user.MangasConfigurations.NSFWBehaviour = updateConfigs.NSFWBehaviour != null ? _mapper.Map<NSFWBehaviourEnum>(updateConfigs.NSFWBehaviour) : user.MangasConfigurations.NSFWBehaviour;
            user.MangasConfigurations.ReadingMode = updateConfigs.ReadingMode != null ? _mapper.Map<ReadingModeTypeEnum>(updateConfigs.ReadingMode) : user.MangasConfigurations.ReadingMode;
            user.MangasConfigurations.TranslationLanguage = updateConfigs.TranslationLanguage != null ? _mapper.Map<TranslationLanguageEnum>(updateConfigs.TranslationLanguage) : user.MangasConfigurations.TranslationLanguage;
            user.MangasConfigurations.SkipChapterToAnotherTranslation = updateConfigs.SkipChapterToAnotherTranslation  ?? user.MangasConfigurations.SkipChapterToAnotherTranslation;
            user.MangasConfigurations.ShowProgressForChaptersWithDecimals = updateConfigs.ShowProgressForChaptersWithDecimals ?? user.MangasConfigurations.ShowProgressForChaptersWithDecimals;

            user = await _userRepo.Update(user);
            return _mapper.Map<UserMangasConfigsDTO>(user.MangasConfigurations);
        }

        public async Task<List<UserDTO>> GetUsers()
        {
            var users = await _userRepo.GetAll();
            return _mapper.Map<List<UserDTO>>(users);
        }
    }
}
