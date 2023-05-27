using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Base
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string DiscordId { get; set; }
        public string Email { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
