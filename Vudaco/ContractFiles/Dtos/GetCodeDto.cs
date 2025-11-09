using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.ContractFiles.Dtos
{
    public class GetCodeDto
    {
        public DateTime yearMonth { get; set; }
        public string Type { get; set; }
        public int StorageId { get; set; }
    }
}
