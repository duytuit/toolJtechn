using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JtechnApi.Users.Dtos
{
    public class CreateRoleDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression("^[A-Za-z0-9][A-Za-z0-9._-]*$")]
        public string Code { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

    public class UpdateRoleDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public bool? IsActive { get; set; }
    }

    public class AssignRolePermissionsDto
    {
        public List<string> PermissionCodes { get; set; } = new List<string>();
    }

    public class AssignEmployeeRolesDto
    {
        public List<string> RoleCodes { get; set; } = new List<string>();
    }
}
