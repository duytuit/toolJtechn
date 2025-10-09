using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares;

namespace Vudaco.Controllers
{
    [ApiController]
    [Route("/")] // <-- Root path
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly VudacoDBContext _context;
        private readonly RedisService _redisService;

        public HomeController(ILogger<HomeController> logger, RedisService redisService, VudacoDBContext context)
        {
            _logger = logger;
            _redisService = redisService;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
       //[HttpGet]
       //public string Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
       //{
            //return "Chào mừng bạn đến với chúng tôi....";
            //var value = _redisService.GetAsync("jtec_hn_database_update_AsyncKTNQ");
            //return value.Result ?? "No value found in Redis";
        //}
        public IActionResult Index()
        {
            ViewData["Message"] = "Chào mừng đến Web API + View2";
            return View();
        }
    }

}
