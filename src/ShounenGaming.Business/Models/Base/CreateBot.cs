using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Models.Base
{
    public class CreateBot
    {
        [Required(AllowEmptyStrings = false)]
        public string DiscordId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [StringLength(10, MinimumLength = 4)]
        public string Password { get; set; }
    }
}
