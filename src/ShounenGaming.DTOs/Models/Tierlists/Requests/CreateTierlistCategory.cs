using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Tierlists.Requests
{
    public class CreateTierlistCategory
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int ImageId { get; set; }
    }
}
