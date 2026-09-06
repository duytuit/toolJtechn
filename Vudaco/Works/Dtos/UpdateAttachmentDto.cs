using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Works.Dtos
{
    public class UpdateAttachmentDto
    {
        public int Id { get; set; }
        public List<FileItemDto> Attachments { get; set; }
    }
}