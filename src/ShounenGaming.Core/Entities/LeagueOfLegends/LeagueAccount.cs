using ShounenGaming.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueAccount : BaseEntity
    {
        public string AccountId { get; set; }
        public int ProfileIconId { get; set; }
        public DateTime RevisionDate { get; set; }
        public string Name { get; set; }
        public string SummonerId { get; set; }
        public string Puuid { get; set; }
        public long SummonerLevel { get; set; }


        public int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual List<LeagueRank> Ranks { get; set; }
    }
}
