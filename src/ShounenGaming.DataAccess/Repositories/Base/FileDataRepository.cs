using Microsoft.EntityFrameworkCore;
using ShounenGaming.Core.Entities.Base;
using ShounenGaming.DataAccess.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Repositories.Base
{
    public class FileDataRepository : BaseRepository<FileData>, IFileDataRepository
    {
        public FileDataRepository(DbContext context) : base(context)
        {
        }

        public override void DeleteDependencies(FileData entity)
        {
            return;
        }
    }
}
