using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Base
{
    public class ServerMemberDTO
    {
        public string DiscordId { get; set; }
        public string DisplayName { get; set; }
        public string ImageUrl { get; set; }
        public string Username { get; set; }
    }
}
