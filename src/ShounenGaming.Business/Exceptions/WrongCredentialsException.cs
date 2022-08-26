using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Exceptions
{
    public class WrongCredentialsException : Exception
    {

        public WrongCredentialsException(string message)
            : base($"Wrong {message}")
        {
        }

    }
}