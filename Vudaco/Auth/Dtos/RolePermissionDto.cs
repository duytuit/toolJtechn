using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Auth.Dtos
{
    public class RolePermissionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StorageId { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public List<PermissionDetail> PermissionDetail { get; set; }
        public bool? Status { get; set; }
        public int RoleId { get; set; }
      
    }
    public class PermissionDetail
    {
        public string Name { get; set; }
        public string permission { get; set; }
        public bool All { get; set; }
        public bool View { get; set; }
        public bool Add { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
      
    }
}
