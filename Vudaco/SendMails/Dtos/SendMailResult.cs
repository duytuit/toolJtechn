using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.SendMails.Dtos
{
  public class SendMailResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public string Error { get; set; }

        public string TemplateCode { get; set; }
        public string SmtpCode { get; set; }
        public string To { get; set; }

        public DateTime SentAt { get; set; }

        // nếu có log DB
        public long? LogId { get; set; }
    }
}
