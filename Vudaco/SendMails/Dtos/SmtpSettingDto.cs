using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.SendMails.Dtos
{
    public class SmtpSettingDto
    {
      public int Id { get; set; }
        public int StorageId { get; set; }
        public string Code { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public bool EnableSsl { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }

    }
}
