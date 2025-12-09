using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.BorrowProducts.Dtos
{
    public class BorrowProductDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public int Status { get; set; }
        public int UserId { get; set; }
        public DateTime? From_date { get; set; }
        public DateTime? To_date { get; set; }

    }
}
