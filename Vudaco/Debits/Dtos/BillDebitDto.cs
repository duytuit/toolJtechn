using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Debits.Dtos
{
    public class BillDebitDto
    {
        public int[] Ids { get; set; }
        public string CusBill { get; set; }
        public DateTime? CusBillDate { get; set; }
        public string SupBill { get; set; }
        public DateTime? SupBillDate { get; set; }
    }
}
