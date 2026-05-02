using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Depreciations.Dtos
{
    public class DepreciationAllocationDto
    {
        public DateTime AccountingDate { get; set; }
        public string CycleName { get; set; }
        public int Type { get; set; }
        public string Note { get; set; }
        public int StorageId { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public int? DeletedBy { get; set; }
    }
}
