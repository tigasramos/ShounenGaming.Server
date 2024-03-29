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

    public class AddedMangaActionRepository : BaseRepository<AddedMangaAction>, IAddedMangaActionRepository
    {
        public AddedMangaActionRepository(DbContext context) : base(context)
        {
        }  
    }
}
