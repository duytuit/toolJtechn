using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.ContractFiles.Dtos
{
    public class ConfirmFileInfoDto
    {
        public int[] Ids { get; set; }
        public int StatusConfirm { get; set; }
        public DateTime? NgayKeoCont { get; set; }
        public DateTime? NgayHetHan { get; set; }
    }
}
