using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using JtechnApi.Shares.Connects;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using JtechnApi.Shares;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("/history-scan")] // <-- Root path
    public class HistoryScanDrawingController : BaseApiController
    {
        private readonly ILogger<HistoryScanDrawingController> _logger;
        private readonly IHttpContextAccessor _accessor;

        public HistoryScanDrawingController(ILogger<HistoryScanDrawingController> logger, IHttpContextAccessor accessor)
        {
            _logger = logger;
            _accessor = accessor;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Post([FromForm] ScanRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Scan))
            {
                var arrayCode = request.Scan.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(x => x.Trim())
                                                   .ToArray();
            
                if (arrayCode.Length == 0)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu: " + request.Scan, null);
                }
            
                string rawCode = arrayCode[0]; // ví dụ: "H03482075 00 0001" hoặc "03482074 00"
            
                // Lọc ra toàn bộ chữ số
                string digitsOnly = new string(rawCode.Where(char.IsDigit).ToArray());
            
                string baseCode = digitsOnly.Length >= 8 ? digitsOnly.Substring(0, 8) : "";
                string param1 = baseCode;
                string param2 = "";
            
                if (int.TryParse(baseCode, out int parsedInt))
                {
                    param2 = parsedInt.ToString();
                }
            
                if (string.IsNullOrWhiteSpace(param1) || string.IsNullOrWhiteSpace(param2))
                {
                    return ApiResponseResult<object>(false, $"Không thể xử lý mã: {rawCode}", null);
                }
                //DataAccess ac = new DataAccess();
                //string querry = "SELECT TOP 10 [id] FROM[SmartManagement].[dbo].[Control_ProgramPlug_Visualize]";
                //var dt = ac.RunQuery(querry);
                //return ApiResponseResult<object>(true, "Không tìm thấy dữ liệu",null);
                string location = Helper.GetClientInfo(_accessor, request.ClientName);
                using (var ac = new DataAccess())
                 {
                     HistoryScanDrawingDto _dto = new HistoryScanDrawingDto();
                     _dto.Lot = param2;
                     _dto.Scan = request.Scan;
                     _dto.Location = location;
                     _dto.Created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                     ac.UpsertFromObject("Control_Assembled_History_Scan_Drawing", _dto,"id");
                     return ApiResponseResult<object>(true, "Thêm thành công", _dto);
                 }
            }
            return ApiResponseResult<object>(false, "Không xử lý được dữ liệu" + request.Scan, null);
        }

    }
    public class ScanRequest
    {
        public string Scan { get; set; }
        public string ClientName { get; set; }
    }
    public class HistoryScanDrawingDto
    {
        public int Id { get; set; }
        public string Lot { get; set; }
        public string Scan { get; set; }
        public string Created_at { get; set; }
        public string Location { get; set; }
    }

}
