using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Works.Dtos
{
    public class WorkHistoryDto
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string Content { get; set; }
        public int StorageId { get; set; }
        public int ModelId { get; set; }
        public string Model { get; set; }
        public int Action { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
