﻿using ShounenGaming.DTOs.Models.Base.Enums;
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
        public string Username { get; set; }
        public string DiscordId { get; set; }
        public string DiscordImage { get; set; }
        public DateTime? Birthday { get; set; }
        public RolesEnumDTO Role { get; set; }
    }
}
