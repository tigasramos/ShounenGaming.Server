using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueChampion : BaseEntity
    {
        public string ChampionId { get; set; }
        public long Key { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string ImageName { get; set; }
        public List<LeagueChampionSkin> Skins { get; set; }
        public string Lore { get; set; }
        public string Blurb { get; set; }
        public string Partype { get; set; }
        public List<LeagueChampionTag> Tags { get; set; }
        public LeagueChampionInfo Info { get; set; }
        public LeagueChampionStats Stats { get; set; }
    }
}
