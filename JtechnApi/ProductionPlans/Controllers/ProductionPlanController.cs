
using JtechnApi.ProductionPlans.Dtos;
using JtechnApi.ProductionPlans.Repositories;
using JtechnApi.Shares.Connects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System;
using System.Linq;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductionPlanController : BaseApiController
    {

        private readonly ConnectionStrings con;
        private readonly IProductionPlanRepository repo;
        private readonly ILogger<ProductionPlanController> _logger;
         private readonly OracleConnection _oracle;

        public ProductionPlanController(ILogger<ProductionPlanController> logger, ConnectionStrings c, IProductionPlanRepository r, OracleConnection oracle)
        {
            _logger = logger;
            con = c;
            repo = r;
            _oracle = oracle;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 20, [FromQuery] RequestPlanDto RequestPlanDto = null)
        {
            if (!string.IsNullOrWhiteSpace(RequestPlanDto.Code))
            {
                var arrayCode = RequestPlanDto.Code.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(x => x.Trim())
                                                   .ToArray();

                if (arrayCode.Length == 0)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu: " + RequestPlanDto.Code, null);
                }

                string rawCode = arrayCode[0]; // ví dụ: "H03482075 00 0001" hoặc "03482074 00"

                // Lọc ra toàn bộ chữ số
                string digitsOnly = new string(rawCode.Where(char.IsDigit).ToArray());

                string baseCode = digitsOnly.Length >= 8 ? digitsOnly.Substring(0, 8) : "";
                string param1 = baseCode + "%";
                string param2 = "";

                if (int.TryParse(baseCode, out int parsedInt))
                {
                    param2 = parsedInt.ToString() + "%";
                }

                if (string.IsNullOrWhiteSpace(param1) || string.IsNullOrWhiteSpace(param2))
                {
                    return ApiResponseResult<object>(false, $"Không thể xử lý mã: {rawCode}", null);
                }

                var command = _oracle.CreateCommand();
                command.CommandText = @"
                    SELECT 品目C, 発注SEQ
                    FROM (
                        SELECT 品目C, 発注SEQ
                        FROM DFW_H10F
                        WHERE 発注SEQ LIKE :param1
                           OR 発注SEQ LIKE :param2
                        ORDER BY 発注SEQ
                    )
                    WHERE ROWNUM = 1
                ";

                command.Parameters.Add(new OracleParameter("param1", OracleDbType.Varchar2, param1, ParameterDirection.Input));
                command.Parameters.Add(new OracleParameter("param2", OracleDbType.Varchar2, param2, ParameterDirection.Input));

                using var reader = await command.ExecuteReaderAsync();
                var table = new DataTable();
                table.Load(reader);

                if (table.Rows.Count == 0)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy mã sản phẩm trên hệ thống.", null);
                }

                RequestPlanDto.Code = table.Rows[0]["品目C"].ToString().Trim();
            }
            var result = await repo.GetPaginatedAsync(RequestPlanDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
    }
}
