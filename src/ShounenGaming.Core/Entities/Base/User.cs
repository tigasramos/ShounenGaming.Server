using ShounenGaming.Core.Entities.Base.Enums;
using ShounenGaming.Core.Entities.LeagueOfLegends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Base
{
    public class User : AuthEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string DiscordId { get; set; }
        public bool DiscordVerified { get; set; }
        public string? DiscordImage { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }
        public bool EmailVerified { get; set; }

        public DateTime Birthday { get; set; }
        public RolesEnum Role { get; set; }

    }
}
