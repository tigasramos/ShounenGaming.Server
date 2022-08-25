using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Base
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DiscordId { get; set; }
        public DateTime Birthday { get; set; }


        public string? RefreshToken { get; set; }
    }
}
