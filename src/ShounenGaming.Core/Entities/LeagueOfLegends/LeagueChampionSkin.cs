using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueChampionSkin : BaseEntity
    {
        public long SkinId { get; set; }
        public int Num { get; set; }
        public string Name { get; set; }
        public bool Chromas { get; set; }
    }
}
