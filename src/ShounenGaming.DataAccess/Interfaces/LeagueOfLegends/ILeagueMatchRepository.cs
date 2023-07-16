using ShounenGaming.Core.Entities.LeagueOfLegends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Interfaces.LeagueOfLegends
{
    public interface ILeagueMatchRepository : IBaseRepository<LeagueMatch>
    {
        Task<LeagueMatch?> GetMatchByGameId(long gameId);
        Task<List<LeagueMatch>> GetAllMatchesFromSummoner(string summonerId);
        Task<List<LeagueMatch>> GetClashMatchesFromSummoner(string summonerId);
    }
}
