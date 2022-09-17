using ShounenGaming.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Tierlists
{
    public class TierlistCategory : BaseEntity
    {
        public string Name { get; set; }

        public int ImageId { get; set; }
        public virtual FileData Image { get; set; }

        public string Description { get; set; }

        public virtual List<Tierlist> Tierlists { get; set; }
    }
}
