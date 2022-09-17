using ShounenGaming.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Tierlists
{
    public class UserTierlist : BaseEntity
    {
        public string Name { get; set; }
        public int TierlistId { get; set; }
        public virtual Tierlist Tierlist { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public virtual List<TierChoice> Choices { get; set; }
    }
}
