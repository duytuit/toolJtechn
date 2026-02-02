
namespace Vudaco.Comments.Dtos
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public int Status { get; set; }
        public int StorageId { get; set; }

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }

        public string Permissions { get; set; }
    }
}
