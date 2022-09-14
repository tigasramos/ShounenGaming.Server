using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Core.Entities.Tierlists
{
    public class Tier : SimpleEntity
    {
        public int Order { get; set; }
        public string Name { get; set; }
        public string ColorHex { get; set; }

    }
}
