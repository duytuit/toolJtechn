using System;

namespace JtecFactory.Models
{
    public class PermissionModel : ISoftDeletable
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MenuCode { get; set; }
        public string FormName { get; set; }
        public bool IsActive { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}
