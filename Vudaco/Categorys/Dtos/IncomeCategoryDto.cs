using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Categorys.Dtos
{
    public class IncomeCategoryDto
    {
        public long Id { get; set; }
        public string IncomeCode { get; set; }
        public string IncomeName { get; set; }
        public string ParentCode { get; set; }
        public int? StorageId { get; set; }
    }
}
