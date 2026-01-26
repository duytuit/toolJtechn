using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.BorrowProducts.Dtos
{
    public class DataSayDto
    {
        public int Id { get; set; }
        public string Lot { get; set; }
        public string Code { get; set; }
        public string Content { get; set; }
        public int? Type { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public string UserBy { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? From_date { get; set; }
        public DateTime? To_date { get; set; }

    }
}
