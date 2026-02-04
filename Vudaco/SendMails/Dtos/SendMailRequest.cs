using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.SendMails.Dtos
{
   public class SendMailRequest
    {
        public string To { get; set; }

        // chọn template
        public string TemplateCode { get; set; }

        // chọn smtp
        public string SmtpCode { get; set; }   // null = default

        // param động
        public Dictionary<string, string> Parameters { get; set; }
    }
}
