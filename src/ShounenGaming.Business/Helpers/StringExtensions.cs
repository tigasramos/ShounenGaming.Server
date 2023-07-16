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
            // TODO: Try this https://stackoverflow.com/questions/10081907/in-c-sharp-how-can-i-prepare-a-string-to-be-valid-for-windows-directory-name
            return str.ToLower().Replace(".", "-").Replace(" ", "-").Replace(":","").Replace("\\", "").Replace("/", "").Replace(";","").Replace("?","").Replace("\"", "");
        }
    }
}
