using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.ProductionPlans.Dtos
{
    public class RequestPlanDto
    {
        public string ProductCode { get; set; }
        public string GiaCong { get; set; }
        public string Code { get; set; }
        public string Cam { get; set; }
        public string Filter_15 { get; set; }
        public string Filter_16 { get; set; }
        public string Filter_17 { get; set; }

    }
}
