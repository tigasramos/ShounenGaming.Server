using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Exceptions
{
    public class SavingImageException : Exception
    {

        public SavingImageException(string message)
            : base(message)
        {
        }

    }
}
