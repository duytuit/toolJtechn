
using System;
using System.Collections.Generic;
using Vudaco.FormRequests.Models;

namespace Vudaco.FormRequests.Dtos
{
    public class FormRequestLeaveDto
    {
        public int Id { get; set; }
        public int StorageId { get; set; }
        public int EmployeeId { get; set; }
        public List<LeaveRequestDto> LeaveRequestDto { get; set; }
        public int? ConfirmBy { get; set; }
        public DateTime? ConfirmAt { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public string Note { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
