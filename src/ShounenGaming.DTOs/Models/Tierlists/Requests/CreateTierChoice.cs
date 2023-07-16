using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Tierlists.Requests
{
    public class CreateTierChoice
    {
        public int TierId { get; set; }
        public List<int> ItemsIds { get; set; }
    }
}
