
namespace Vudaco.Notifys.Dtos
{
    public class NotifyDto
    {
        public int Id { get; set; }
        public int StorageId { get; set; }
        public int PostId { get; set; }
        public int EmployeeId { get; set; }
        public string Screen { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public string? Image { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
