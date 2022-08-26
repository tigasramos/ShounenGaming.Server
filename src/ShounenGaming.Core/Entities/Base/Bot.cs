﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Base
{
    public class Bot : AuthEntity
    {
        public string DiscordId { get; set; }
        public string PasswordHashed { get; set; }
        public string Salt { get; set; }

    }
}