using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Partners.Dtos
{
    public class PartnerDetailDto
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public bool IsSupplier { get; set; }
        public string Code { get; set; }
        public string Note { get; set; }
        public int StorageId { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
