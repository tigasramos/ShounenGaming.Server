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
        public string ImageUrl { get; set; }
        public virtual User Creator { get; set; }
        public virtual TierlistCategory Category { get; set; }
        public virtual List<TierlistItem> Items { get; set; }
        public virtual List<Tier> DefaultTiers { get; set; }
    }
}
