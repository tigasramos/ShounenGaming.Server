using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueTeamObjective : SimpleEntity
    {
        public int Kills { get; set; }
        public bool First { get; set; }
    }
}
