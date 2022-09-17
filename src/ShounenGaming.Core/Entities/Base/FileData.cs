using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Base
{
    public class FileData : BaseEntity
    {
        public string FileId { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
    }
}
