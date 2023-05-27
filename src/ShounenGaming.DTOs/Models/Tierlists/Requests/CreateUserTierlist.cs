using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Tierlists.Requests
{
    public class CreateUserTierlist
    {
        public int TierlistId { get; set; }
        public string Name { get; set; }
    }
}
