using ShounenGaming.Core.Entities.Base.Enums;
using ShounenGaming.Core.Entities.LeagueOfLegends;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Base
{
    public class User : AuthEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public bool DiscordVerified { get; set; }

        public string Username { get; set; }

        public DateTime Birthday { get; set; }

        public bool IsInServer => ServerMemberId != null;

        public int? ServerMemberId { get; set; }
        public virtual ServerMember? ServerMember { get; set; }
    }
}
