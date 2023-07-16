using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueTeamObjectives : SimpleEntity
    {
        public virtual LeagueTeamObjective Baron { get; set; }
        public virtual LeagueTeamObjective Champion { get; set; }
        public virtual LeagueTeamObjective Dragon { get; set; }
        public virtual LeagueTeamObjective Inhibitor { get; set; }
        public virtual LeagueTeamObjective RiftHerald { get; set; }
        public virtual LeagueTeamObjective Tower { get; set; }
    }
}
