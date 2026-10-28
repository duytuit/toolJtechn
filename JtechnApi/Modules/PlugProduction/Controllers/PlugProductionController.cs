using JtechnApi.Modules.PlugProduction.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace JtechnApi.Modules.PlugProduction.Controllers
{
    public class PlugProductionController : Controller
    {
        private readonly PlugProductionRepository _repository;

        public PlugProductionController(PlugProductionRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("/plug-production")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Giám sát sản lượng công đoạn Cắm";
            return View("~/Views/Home/PlugProductionDashboard.cshtml");
        }

        [HttpGet("/api/plug-production/data")]
        public IActionResult Data()
        {
            return Ok(_repository.GetSnapshot());
        }
    }
}
