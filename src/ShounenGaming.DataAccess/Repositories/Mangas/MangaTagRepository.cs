﻿using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Mangas;
using ShounenGaming.DataAccess.Interfaces.Mangas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Repositories.Mangas
{

    public class MangaTagRepository : BaseRepository<MangaTag>, IMangaTagRepository
    {
        public MangaTagRepository(DbContext context) : base(context)
        {
        }

        public override void DeleteDependencies(MangaTag entity)
        {
            return;
        }
    }
}