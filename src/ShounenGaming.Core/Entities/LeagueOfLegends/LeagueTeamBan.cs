using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueTeamBan : SimpleEntity
    {
        public int ChampionId { get; set; }
        public virtual LeagueChampion Champion { get; set; }

        public int PickTurn { get; set; }
    }
}
