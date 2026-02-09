
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Auth.Models
{
    [Table("role_permissions")]
    public class RolePermission
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("permission_id")]
        public int PermissionId { get; set; }
        
        [NotMapped]
        public string PermissionName { get; set; }

        [Required]
        [Column("role_id")]
        public int RoleId { get; set; }

        [Required]
        [Column("all")]
        public bool All { get; set; }

        [Required]
        [Column("view")]
        public bool View { get; set; }

        [Required]
        [Column("add")]
        public bool Add { get; set; }

        [Required]
        [Column("edit")]
        public bool Edit { get; set; }

        [Required]
        [Column("delete")]
        public bool Delete { get; set; }
    }
}
