using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Depreciations.Dtos
{
    public class ListDepreciationDto
    {
       public List<DepreciationDto> Data { get; set; }
       public int StorageId { get; set; }
       public int Type { get; set; }

    }
}
