
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Models.Tierlists
{
    public class TierChoiceDTO
    {
        public TierDTO Tier { get; set; }
        public List<TierlistItemDTO> Items { get; set; }
    }
}
