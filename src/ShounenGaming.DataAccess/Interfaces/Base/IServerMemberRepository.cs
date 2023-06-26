using ShounenGaming.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Interfaces.Base
{
    public interface IServerMemberRepository : IBaseRepository<ServerMember>
    {
        Task<ServerMember?> GetMemberByDiscordId(string discordId);
        Task<List<ServerMember>> GetUnregisteredServerMembers();
    }
}
