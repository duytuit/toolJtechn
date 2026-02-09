
using System.Collections.Generic;

namespace Vudaco.Notifys.Dtos
{
    public class FcmJobDto
    {
        public List<int> UserIds { get; set; } = new();
        public string Title { get; set; }
        public string Body { get; set; }
        public string Screen { get; set; }
        public int StorageId { get; set; }
        public int PostId { get; set; }
        public int Type { get; set; }
        public Dictionary<string, string> Data { get; set; } = new();
    }
}
