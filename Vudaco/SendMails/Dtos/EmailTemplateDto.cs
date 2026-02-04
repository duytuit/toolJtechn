using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.SendMails.Dtos
{
    public class EmailTemplateDto
    {
       public int Id { get; set; }
        public int StorageId { get; set; }
        public string Code { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }

    }
}
