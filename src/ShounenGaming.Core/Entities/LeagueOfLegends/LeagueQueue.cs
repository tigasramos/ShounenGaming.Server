using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueQueue : BaseEntity
    {
        public int QueueId { get; set; }
        public string Map { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
    }
}
