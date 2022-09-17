using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Models.Tierlists.Requests
{
    public class EditTier
    {
        public int Id { get; set; }
        public int? Order { get; set; }
        public string? Name { get; set; }
        public string? ColorHex { get; set; }
    }
}
