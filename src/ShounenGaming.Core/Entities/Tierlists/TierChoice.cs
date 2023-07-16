using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Tierlists
{
    public class TierChoice : BaseEntity
    {
        public virtual Tier Tier { get; set; }
        public virtual List<TierlistItem> Items { get; set; }
    }
}
