using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Debits.Dtos
{
    public class CongNoTongHopDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public int DVDK { get; set; }
        public int CHDK { get; set; }
        public int TTDVDK { get; set; }
        public int TTCHDK { get; set; }
        public int DVTK { get; set; }
        public int CHTK { get; set; }
        public int TTDVTK { get; set; }
        public int TTCHTK { get; set; }
        public int DVCK { get; set; }
        public int CHCK { get; set; }
        public int CK { get; set; }
    }
}
