using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Debits.Dtos
{
    public class ConfirmFileDto
    {
        public int Id { get; set; }
        public int? FileInfoId { get; set; }
        public int DebitId { get; set; }
        public int? PartnerDetailId { get; set; }
        public DateTime AccountingDate { get; set; }
        public int StorageId { get; set; }
        public string FileNumber { get; set; }
        public int? Status { get; set; }
        public int StatusConfirm { get; set; }
        public string Note { get; set; }
        public string Data { get; set; }
        public List<DebitDto> DebitDtos { get; set; }
        public List<serviceDto> Chiphikhac { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
