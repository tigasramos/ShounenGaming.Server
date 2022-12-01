﻿using ShounenGaming.Business.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Models.Tierlists
{
    public class SimpleTierlistDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageId { get; set; }
        public SimpleUserDTO User { get; set; }
    }
}