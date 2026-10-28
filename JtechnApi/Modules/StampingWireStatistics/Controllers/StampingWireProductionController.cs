using JtechnApi.Modules.StampingWireStatistics.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace JtechnApi.Modules.StampingWireStatistics.Controllers
{
    public class StampingWireProductionController : Controller
    {
        private readonly StampingWireProductionRepository _repository;

        public StampingWireProductionController(StampingWireProductionRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("/stamping-wire-production")]
        public IActionResult Index()
        {
            return Dashboard("dap", "Bên Dập", "MT-D");
        }

        [HttpGet("/stamping-wire-production/dap")]
        public IActionResult Stamping()
        {
            return Dashboard("dap", "Bên Dập", "MT-D");
        }

        [HttpGet("/stamping-wire-production/noi")]
        public IActionResult Joining()
        {
            return Dashboard("noi", "Bên Nối", "MT-N");
        }

        [HttpGet("/stamping-wire-production/xoan")]
        public IActionResult Twisting()
        {
            return Dashboard("xoan", "Bên Xoắn", "MT-X");
        }

        [HttpGet("/api/stamping-wire-production/data")]
        public IActionResult Data()
        {
            return Ok(_repository.GetSnapshot());
        }

        [HttpGet("/api/stamping-wire-production/stamping-actual")]
        public IActionResult StampingActual()
        {
            return Ok(new { TotalActual = _repository.GetStampingActualForToday() });
        }

        private IActionResult Dashboard(string area, string areaName, string machinePrefix)
        {
            ViewData["Title"] = $"Giám sát sản lượng máy dập dây - {areaName}";
            ViewData["Area"] = area;
            ViewData["AreaName"] = areaName;
            ViewData["MachinePrefix"] = machinePrefix;
            return View("~/Views/Home/StampingWireProductionDashboard.cshtml");
        }
    }
}
