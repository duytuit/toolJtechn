using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using JtechnApi.Shares;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("/")] // <-- Root path
    public class HomeController : Controller
    {
        private readonly OracleConnection _conn_oracle;
        private readonly ILogger<HomeController> _logger;

        // Inject từ DI
        private readonly RedisService _redisService;

        public HomeController(ILogger<HomeController> logger, OracleConnection conn_oracle, RedisService redisService)
        {
            _logger = logger;
            _conn_oracle = conn_oracle;
            _redisService = redisService;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            return "Chào mừng bạn đến với chúng tôi....";
            //var value = _redisService.GetAsync("jtec_hn_database_update_AsyncKTNQ");
            //return value.Result ?? "No value found in Redis";
        }
       //public IActionResult Index()
       //{
       //    var cmd = _conn_oracle.CreateCommand();
       //    cmd.CommandText = "SELECT 場所c,棚番 FROM TAD_Z60M WHERE 品目C = 'W FLRY-B0.5RB'";
       //
       //    var reader = cmd.ExecuteReader();
       //    var table = new DataTable();
       //    table.Load(reader);
       //    // Duyệt từng dòng (nếu muốn hiển thị)
       //    foreach (DataRow row in table.Rows)
       //    {
       //        var i= row;
       //    }
       //
       //    DataAccess ac = new DataAccess();
       //    ViewData["Message"] = "Chào mừng đến Web API + View";
       //    string querry = "SELECT TOP 10 [id] FROM[SmartManagement].[dbo].[Control_ProgramPlug_Visualize]";
       //    var dt = ac.RunQuery(querry);
       //    return View();
       //}
    }
}
