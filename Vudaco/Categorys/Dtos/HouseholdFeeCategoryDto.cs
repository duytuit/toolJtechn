using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Categorys.Dtos
{
    public class HouseholdFeeCategoryDto
    {
        public long Id { get; set; }
        public string HouseholdCode { get; set; }
        public string HouseholdName { get; set; }
        public int? StorageId { get; set; }
        public double? Amount { get; set; }
    }
}
