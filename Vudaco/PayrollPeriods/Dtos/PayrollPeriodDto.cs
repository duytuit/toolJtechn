
namespace Vudaco.PayrollPeriods.Dtos
{
    public class PayrollPeriodDto
    {
        public int Id { get; set; }
        public int StorageId { get; set; }
        public int EmployeeId { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }
        public string? CycleName { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
