using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.ContractFiles.Dtos
{
    public class FileInfoDto
    {
        public int Id { get; set; }
        public int? PartnerDetailId { get; set; }
        public int StorageId { get; set; }
        public DateTime AccountingDate { get; set; }
        public string FileNumber { get; set; }
        public string Declaration { get; set; }
        public string Bill { get; set; }
        public string Quantity { get; set; }
        public string ContainerCode { get; set; }
        public string Sales { get; set; }
        public int? Type { get; set; }
        public int? Feature { get; set; }
        public int? DeclarationQuantity { get; set; }
        public int? DeclarationType { get; set; }
        public int? Business { get; set; }
        public int? Occurrence { get; set; }
        public string Note { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int[] EmployeeIds { get; set; }

    }
}
