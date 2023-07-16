using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Tierlists
{
    public class TierDTO
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public string ColorHex { get; set; }
    }
}
