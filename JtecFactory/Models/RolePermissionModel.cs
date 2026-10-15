using System;

namespace JtecFactory.Models
{
    public class RolePermissionModel
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public DateTime created_at { get; set; }
    }
}
