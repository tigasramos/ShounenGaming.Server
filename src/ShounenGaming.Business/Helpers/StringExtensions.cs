using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Helpers
{
    public static class StringExtensions
    {
        public static string NormalizeStringToDirectory(this string str)
        {
            return str.ToLower().Replace(".", "-").Replace(" ", "-").Replace(":","").Replace(";","").Replace("?","");
        }
    }
}
