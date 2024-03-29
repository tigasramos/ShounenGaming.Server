﻿using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.Core.Entities.Mangas.Enums;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Repositories.Mangas
{
    public class MangaUserDataRepository : BaseRepository<MangaUserData>, IMangaUserDataRepository
    {
        public MangaUserDataRepository(DbContext context) : base(context)
        {
        }

        public override void DeleteDependencies(MangaUserData data)
        {
            context.RemoveRange(data.ChaptersRead);
        }

        public async Task<List<MangaUserData>> GetMangasByStatusByUser(MangaUserStatusEnum status, int userId)
        {
            return await dbSet.Where(c => c.Status == status && c.User.Id == userId).ToListAsync();
        }

        public async Task<List<MangaUserData>> GetByUser(int userId)
        {
            return await dbSet.Where(m => m.User.Id == userId).ToListAsync();
        }

        public async Task<MangaUserData?> GetByUserAndManga(int userId, int mangaId)
        {
            return await dbSet.FirstOrDefaultAsync(m => m.User.Id == userId && m.Manga.Id == mangaId);
        }

       
    }
}