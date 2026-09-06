using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Works.Dtos
{
    public class CheckListDto
    {
        public int Id { get; set; }
        public int WorkId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StorageId { get; set; }
        public bool Checked { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
