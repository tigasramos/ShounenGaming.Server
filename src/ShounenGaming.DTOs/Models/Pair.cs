using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models
{
    public class Pair<T, U> 
    {
        public T Id { get; set; }
        public U Value { get; set; }
    }
}
