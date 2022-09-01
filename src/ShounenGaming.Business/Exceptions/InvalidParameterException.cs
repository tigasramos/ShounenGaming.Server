using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Exceptions
{
    public class InvalidParameterException : Exception
    {
        public InvalidParameterException(string parameter, string message) : base($"Invalid {parameter}: {message}")
        {
        }
    }
}