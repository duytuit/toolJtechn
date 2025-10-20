using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Activitys.Dtos
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? ContentId { get; set; }
        public string ContentType { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string OldData { get; set; }
        public string NewData { get; set; }
        public string Sql { get; set; }
        public string IpAddress { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
