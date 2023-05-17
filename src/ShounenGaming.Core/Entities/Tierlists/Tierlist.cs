using ShounenGaming.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Tierlists
{
    public class Tierlist : BaseEntity
    {
        public string Name { get; set; }

        public int ImageId { get; set; }
        public string ImageName { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int CategoryId { get; set; }
        public virtual TierlistCategory Category { get; set; }

        public virtual List<TierlistItem> Items { get; set; }
        public virtual List<Tier> DefaultTiers { get; set; }
    }
}
