using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JtechnApi.Requireds.Models;

namespace JtechnApi.UploadKTNQ.Dtos
{
    public class AddKTNQDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Content { get; set; }
    }
}
