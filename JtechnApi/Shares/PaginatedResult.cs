using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.Shares
{
    public class PaginatedResult<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public List<T> Items { get; set; }
    }
}
