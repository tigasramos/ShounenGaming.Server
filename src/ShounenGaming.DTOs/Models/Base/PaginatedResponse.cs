using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DTOs.Models.Base
{
    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; }
        public int CurrentPage { get; set; }
        public int MaxCount { get; set; }
    }
}
