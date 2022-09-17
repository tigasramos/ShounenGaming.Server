using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Models.Tierlists.Requests
{
    public class CreateTierlist
    {
        public string Name { get; set; }
        public int ImageId { get; set; }
        public int CategoryId { get; set; }
        public List<CreateTier> DefaultTiers { get; set; }
    }
}
