using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Base
{
    public class Bot : BaseEntity
    {
        public string DiscordId { get; set; }
        public string PasswordHashed { get; set; }
        public string Salt { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryDate { get; set; }
    }
}
