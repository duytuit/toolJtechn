using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Categorys.Dtos
{
    public class PriceCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StorageId { get; set; }
        public int? PartnerDetailId { get; set; }
        public int SellingPrice { get; set; }
        public int PurchasePrice { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        
    }
}
