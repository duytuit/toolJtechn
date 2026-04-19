using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Depreciations.Dtos
{
    public class DepreciationAllocationDetailDto
    {
        public int Id { get; set; }

        public int? DepreciationId { get; set; }

        public int? DepreciationAllocationId { get; set; }

        public decimal MonthlyDepreciation { get; set; }

        public int? StorageId { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public int? DeletedBy { get; set; }
    }
}
