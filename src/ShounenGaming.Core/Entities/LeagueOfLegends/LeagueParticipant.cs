using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.LeagueOfLegends
{
    public class LeagueParticipant : SimpleEntity
    {
        public int ParticipantId { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public bool FirstBloodKill { get; set; }
        public bool FirstBloodAssist { get; set; }
        public bool FirstTowerKill { get; set; }
        public bool FirstTowerAssist { get; set; }
        public int ChampionLevel { get; set; }
        public int ChampionExperience { get; set; }
        public string Lane { get; set; }
        public string Role { get; set; }
        public string IndividualPosition { get; set; }
        public string TeamPosition { get; set; }
        public int TeamId { get; set; }
        public int GoldEarned { get; set; }
        public int GoldSpent { get; set; }

        public string Puuid { get; set; }
        public int ProfileIcon { get; set; }
        public string SummonerId { get; set; }
        public string SummonerName { get; set; }

        public int ChampionId { get; set; }
        public virtual LeagueChampion Champion { get; set; }

        public virtual LeagueParticipantPerks Perks { get; set; }

        public bool GameEndedInEarlySurrender { get; set; }
        public bool GameEndedInSurrender { get; set; }
        public int LongestTimeSpentLiving { get; set; }
        public int TotalTimeSpentDead { get; set; }

        //Damage
        public int DamageDealtToBuilding { get; set; }
        public int DamageDealtToObjectives { get; set; }
        public int DamageDealtToTurrets { get; set; }
        public int DamageSelfMitigated { get; set; }
        public int MagicDamageDealt { get; set; }
        public int MagicDamageDealtToChampions { get; set; }
        public int MagicDamageTaken { get; set; }
        public int PhysicalDamageDealt { get; set; }
        public int PhysicalDamageDealtToChampions { get; set; }
        public int PhysicalDamageTaken { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageDealtToChampions { get; set; }
        public int TotalDamageShieldedOnTeammates { get; set; }
        public int TotalDamageTaken { get; set; }
        public int TrueDamageDealt { get; set; }
        public int TrueDamageDealtToChampions { get; set; }
        public int TrueDamageTaken { get; set; }
        public int TotalHeal { get; set; }
        public int TotalHealsOnTeammates { get; set; }
        public int TimeCCingOthers { get; set; }

        //CS
        public int NeutralMinionsKilled { get; set; }
        public int TotalMinionsKilled { get; set; }

        //Vision
        public int VisionWardsPlaced { get; set; }
        public int VisionWardsBoughtInGame { get; set; }
        public int WardsPlaced { get; set; }
        public int WardsKilled { get; set; }
        public int VisionScore { get; set; }

        //Multi Kills
        public int KillingSprees { get; set; }
        public int LargestKillingSpree { get; set; }
        public int DoubleKills { get; set; }
        public int TripleKills { get; set; }
        public int QuadraKills { get; set; }
        public int PentaKills { get; set; }

        //Objectives
        public int DragonKills { get; set; }
        public int InhibitorKills { get; set; }
        public int InhibitorTakedowns { get; set; }
        public int InhibitorsLost { get; set; }
        public bool KilledNexus { get; set; }
        public int ObjectivesStolen { get; set; }
        public int ObjectivesStolenAssists { get; set; }
        public int TurretKills { get; set; }
        public int TurretTakedowns { get; set; }
        public int BaronKills { get; set; }

        //Items
        public int Item0 { get; set; }
        public int Item1 { get; set; }
        public int Item2 { get; set; }
        public int Item3 { get; set; }
        public int Item4 { get; set; }
        public int Item5 { get; set; }
        public int Item6 { get; set; }
        public int ItemsPurchased { get; set; }

        //Spells
        public int Spell1Casts { get; set; }
        public int Spell2Casts { get; set; }
        public int Spell3Casts { get; set; }
        public int Spell4Casts { get; set; }
        public int Summoner1Id { get; set; }
        public int Summoner1Casts { get; set; }
        public int Summoner2Id { get; set; }
        public int Summoner2Casts { get; set; }
    }
}
