using JtechnApi.Modules.AssemblyTargetStatistics.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace JtechnApi.Modules.AssemblyTargetStatistics.Controllers
{
    public class AssemblyCombineTargetStatisticsController : Controller
    {
        private readonly AssemblyCombineTargetStatisticsRepository _repository;

        public AssemblyCombineTargetStatisticsController(
            AssemblyCombineTargetStatisticsRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("/assembled-combine-target")]
        public IActionResult Index()
        {
            return View("~/Views/Home/AssemblyCombineTargetStatistics.cshtml");
        }

        [HttpGet("/api/assembled-combine-target/statistics")]
        public IActionResult Statistics([FromQuery] DateTime? date)
        {
            return Ok(_repository.GetSnapshot(date ?? DateTime.Today));
        }
    }
}
