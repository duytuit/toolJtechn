using JtechnApi.Modules.CutProduction.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace JtechnApi.Modules.CutProduction.Controllers
{
    public class CutProductionController : Controller
    {
        private readonly CutProductionRepository _repository;

        public CutProductionController(CutProductionRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("/cut-production")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Giám sát sản lượng công đoạn Cắt";
            return View("~/Views/Home/CutProductionDashboard.cshtml");
        }

        [HttpGet("/api/cut-production/data")]
        public IActionResult Data()
        {
            return Ok(_repository.GetSnapshot());
        }
    }
}
