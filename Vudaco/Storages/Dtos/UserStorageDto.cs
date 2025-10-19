using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Storages.Dtos
{
    public class UserStorageDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StorageId { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
