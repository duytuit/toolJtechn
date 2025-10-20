using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Categorys.Dtos
{
    public class FundCategoryDto
    {
        public long Id { get; set; }
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public int? StorageId { get; set; }
    }
}
