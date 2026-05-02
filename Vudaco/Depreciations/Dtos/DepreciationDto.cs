using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Depreciations.Dtos
{
    public class DepreciationDto
    {
        public int Id { get; set; }

        public string CodeNumber { get; set; }

        public string Name { get; set; }

        public int? OriginalCost { get; set; }

        public int? UsefulLife { get; set; }

        public DateTime? EndDate { get; set; }

        public int? MonthlyDepreciation { get; set; }

        public int StorageId { get; set; }

        public int Type { get; set; }
        public int Status { get; set; }
        public string Note { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public int? DeletedBy { get; set; }
    }
}
