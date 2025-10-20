using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Categorys.Dtos
{
    public class AdministrativeFeeCategoryDto
    {
        public long Id { get; set; }
        public string FeeCode { get; set; }
        public string FeeName { get; set; }
        public int? StorageId { get; set; }
        public double? Amount { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
