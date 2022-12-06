using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueMap : BaseEntity
    {
        public int MapId { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
    }
}
