using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueMatch : BaseEntity
    {
        public string PlatformId { get; set; }
        public long GameId { get; set; }
        public DateTime GameCreation { get; set; }
        public TimeSpan GameDuration { get; set; }
        public DateTime GameStartTimestamp { get; set; }
        public DateTime GameEndTimestamp { get; set; }
        public string GameVersion { get; set; }

        public virtual List<LeagueParticipant> Participants { get; set; }
        public virtual List<LeagueTeam> Teams { get; set; }


        public virtual LeagueQueue Queue { get; set; }
        public virtual LeagueGameMode GameMode { get; set; }
        public virtual LeagueGameType GameType { get; set; }
        public virtual LeagueMap Map { get; set; }

    }
}
