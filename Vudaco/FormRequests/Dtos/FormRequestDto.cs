
using System;

namespace Vudaco.FormRequests.Dtos
{
    public class FormRequestDto
    {
        public int Id { get; set; }
        public int StorageId { get; set; }
        public int EmployeeId { get; set; }
        public string Description { get; set; }
        public int? ConfirmBy { get; set; }
        public DateTime? ConfirmAt { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public string Note { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
