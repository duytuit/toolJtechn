using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Auth.Models
{
    [Table("role_permission")]
    public class RolePermission
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        public int PermissionId { get; set; }

        public int RoleId { get; set; }
    }
}
