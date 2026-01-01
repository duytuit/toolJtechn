using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Storages.Dtos
{
    public class StorageDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string Address { get; set; }
        public int? UserId { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
