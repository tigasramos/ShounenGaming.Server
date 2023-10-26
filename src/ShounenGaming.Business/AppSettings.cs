using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business
{
    public class AppSettings
    {
        public string SeqServer { get; set; }
        public string JwtSecret { get; set; }
        public DiscordBotSettings DiscordBot { get; set; }
    }

    public class DiscordBotSettings
    {
        public string DiscordId { get; set; }
        public string Password { get; set; }
    }
}
