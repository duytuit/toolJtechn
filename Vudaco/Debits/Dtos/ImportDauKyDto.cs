using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Debits.Dtos
{
    public class ImportDauKyDto
    {
        public DateTime AccountingDate { get; set; }
        public int StorageId { get; set; }
        public string Data { get; set; }
    }
}
