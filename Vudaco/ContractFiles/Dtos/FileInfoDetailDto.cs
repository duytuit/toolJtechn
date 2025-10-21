using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.ContractFiles.Dtos
{
    public class FileInfoDetailDto
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public int? EmployeeId { get; set; }
        public double? Price { get; set; }
        public int? StorageId { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public string SalesIds { get; set; }
    }
}
