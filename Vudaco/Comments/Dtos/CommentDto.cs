
using Vudaco.Comments.Models;

namespace Vudaco.Comments.Dtos
{
    public class CommentDto
    {
        public int Id { get; set; }
        public int StorageId { get; set; }
        public int PostId { get; set; }
        public int Type { get; set; }
        public string? Message { get; set; }
        public AttachmentInfo AttachmentInfo { get; set; }
        public int? ParentId { get; set; }
        public int EmployeeId { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
