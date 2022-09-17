using ShounenGaming.Business.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Models.Tierlists
{
    public class TierlistDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageId { get; set; }
        public SimpleUserDTO User { get; set; }
        public TierlistCategoryDTO Category { get; set; }
        public List<TierlistItemDTO> Items { get; set; }
        public List<TierDTO> DefaultTiers { get; set; }
    }
}
