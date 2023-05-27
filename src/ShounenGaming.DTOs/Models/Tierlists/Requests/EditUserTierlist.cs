using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Tierlists.Requests
{
    public class EditUserTierlist
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<CreateTierChoice> Choices { get; set; }
    }
}
