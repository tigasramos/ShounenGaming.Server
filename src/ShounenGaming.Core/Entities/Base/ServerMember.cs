using ShounenGaming.Core.Entities.Base.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Base
{
    public class ServerMember: BaseEntity
    {
        public string DiscordId { get; set; }
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string ImageUrl { get; set; }
        public RolesEnum Role { get; set; }

        public virtual User? User { get; set; }
    }
}
