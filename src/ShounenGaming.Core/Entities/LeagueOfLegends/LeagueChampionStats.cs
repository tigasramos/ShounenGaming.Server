using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueChampionStats : BaseEntity
    {
        public double Hp { get; set; }
        public double HpPerLevel { get; set; }
        public double Mp { get; set; }
        public double MpPerLevel { get; set; }
        public double Movespeed { get; set; }
        public double Armor { get; set; }
        public double ArmorPerLevel { get; set; }
        public double Spellblock { get; set; }
        public double SpellblockPerLevel { get; set; }
        public double AttackRange { get; set; }
        public double HpRegen { get; set; }
        public double HpRegenPerLevel { get; set; }
        public double Crit { get; set; }
        public double CritPerLevel { get; set; }
        public double AttackDamage { get; set; }
        public double AttackDamagePerLevel { get; set; }
        public double AttackSpeed { get; set; }
        public double AttackSpeedPerLevel { get; set; }
    }
}
