

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using JtechnApi.Requireds.Models;
using JtechnApi.Requireds.Repositories;
using JtechnApi.Shares.Connects;
using JtechnApi.Accessorys.Models;
using JtechnApi.Shares.BaseRepository;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System;
using Newtonsoft.Json;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TempRequiredController : BaseApiController
    {

        private readonly ConnectionStrings con;
        private readonly ITempRequiredRepository repo;
        private readonly ILogger<ProductionPlanController> _logger;
        private readonly OracleConnection _oracle;
        private readonly DBContext _context;

        public TempRequiredController(ILogger<ProductionPlanController> logger, ConnectionStrings c, ITempRequiredRepository r, DBContext context, OracleConnection oracle)
        {
            _logger = logger;
            con = c;
            repo = r;
            _context = context;
             _oracle = oracle;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// 
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, int pageSize = 400,[FromQuery]RequestRequiredDto RequestRequiredDto = null)
        {
            // if(RequestRequiredDto.Code_nv == null || RequestRequiredDto.Code_nv == "")
            // {
            //     return ApiResponseResult<object>(false, "Vui lòng nhập mã nhân viên", null);
            // }
            if(RequestRequiredDto.Created_at  == null)
            {
                return ApiResponseResult<object>(false, "Vui lòng nhập ngày tạo", null);
            }
            var result = await repo.GetPaginatedAsync(RequestRequiredDto,page, pageSize);
            if (result == null || result.Items.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tempRequired = await repo.GetTempRequiredByIdAsync(id);
            if (tempRequired == null)
            {
                return NotFound();
            }
            return Ok(tempRequired);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromForm] RequiredDto RequiredDto)
        {
             Accessory accessory = _context.Accessory.Where(x =>x.Deleted_at == null && x.Code == RequiredDto.Code).FirstOrDefault();
            if (accessory == null)
            {
                return ApiResponseResult<object>(false, "Mã Linh kiện không tồn tại", null);
            }
            // lấy ra tồn kho của phụ kiện
            var cmd = _oracle.CreateCommand();
            string sql = "SELECT 現在在庫数 FROM V_DFW_Z11_040QF_0 WHERE 品目C ='"+RequiredDto.Code+"' AND 場所C = '0111'";
            cmd.CommandText = sql;
            var reader = cmd.ExecuteReader();
            var table = new DataTable();
            table.Load(reader);
            if (table.Rows.Count == 0)
            {
                return ApiResponseResult<object>(false,"Không tìm thấy Mã Linh kiện trong kho", null);
            }
            // Lấy giá trị hiện tại trong cột 現在在庫数
            var currentStock = table.Rows[0]["現在在庫数"];
            if (currentStock == DBNull.Value || Convert.ToInt32(currentStock) <= 0)
            {
                return ApiResponseResult<object>(false, "Mã Linh kiện không đủ trong kho", null);
            }
            string sql_1 = "SELECT 場所c,棚番 FROM TAD_Z60M WHERE 品目C = '"+RequiredDto.Code+"'";
            cmd.CommandText = sql_1;
            var reader_1 = cmd.ExecuteReader();
            var table_1 = new DataTable();
            table_1.Load(reader_1);
            if (table_1.Rows.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy vị trí trong kho", null);
            }
            string _location = null;
            string _location_order = null;
           
            // lấy id bô phận theo tên
            var department = _context.Department.Where(x => x.Deleted_at == null && x.Name == RequiredDto.Department).FirstOrDefault();
            if (department == null)
            {
                return ApiResponseResult<object>(false, "Bộ phận không tồn tại", null);
            }
            // lấy thông tin nhân viên theo code_nv
            var employee = _context.Employee.Where(x => x.Deleted_at == null && x.Code == RequiredDto.Code_nv).FirstOrDefault();
            if (employee == null)
            {
                return ApiResponseResult<object>(false, "Nhân viên không tồn tại", null);
            }
            foreach (DataRow row in table_1.Rows)
            {
                // Duyệt từng dòng nếu cần thiết
                // var i = row; // Bạn có thể làm gì đó với từng dòng ở đây
                // Ví dụ: lấy giá trị của cột "場所c" và "棚番"
                string locationValue = row["場所c"].ToString().Trim();
                string shelfValue = row["棚番"].ToString().Trim();
                if (locationValue.Contains("0111") && !string.IsNullOrEmpty(locationValue))
                {
                    _location = shelfValue;
                }
                //vị trí bộ phận order
                if(locationValue.Contains(department.Code.ToString()) && !string.IsNullOrEmpty(locationValue)){
                    _location_order = shelfValue;
                }
            }
            if (string.IsNullOrEmpty(_location))
            {
                return ApiResponseResult<object>(false, "Không tìm thấy vị trí trong kho", null);
            }
            // tạo một biến object kiểu any cho tôi 
            // và gán giá trị cho nó
            var anyObject = new
            {
                department_id = department.Id,
                created_by = employee.First_name + " " + employee.Last_name,
                code_nv = RequiredDto.Code_nv,
                selecMachine = "",
                searchcode =  "",
                code = RequiredDto.Code,
                type = RequiredDto.Type,
                quantity = RequiredDto.Quantity,
                usage_status = RequiredDto.Usage_status,
                size =  RequiredDto.Size,
                material_norms = accessory.Material_norms,
                unit = accessory.Unit,
                location_order =  _location_order,
                ton_kho = Convert.ToInt32(currentStock),
                ton_xuong = 0,
                location = _location,
                content = RequiredDto.Content,
                department = RequiredDto.Department,
                print = RequiredDto.Print == 1 ? true : false,
                pc_name = RequiredDto.Pc_name,
            };
            TempRequired tempRequired = new TempRequired
            {
                Msp = accessory.Code,
                Code_nv = RequiredDto.Code_nv,
                Content = JsonConvert.SerializeObject(anyObject),
                Status = 0, // nếu print = 1 thì status = 1, ngược lại status = 0
            };
           var createdTempRequired = await repo.CreateTempRequiredAsync(tempRequired);
           if (createdTempRequired == null) 
           {
               return ApiResponseResult<object>(false, "Thêm mới thất bại", null);
           }
            return ApiResponseResult<object>(true, "Thêm mới thành công", createdTempRequired);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] TempRequired tempRequired)
        {
            if (tempRequired == null || tempRequired.Id != id)
            {
                return BadRequest("TempRequired cannot be null and ID must match");
            }

            var updatedTempRequired = await repo.UpdateTempRequiredAsync(tempRequired);
            if (updatedTempRequired == null)
            {
                return NotFound();
            }
            return Ok(updatedTempRequired);
        }
        [HttpDelete("{id}")]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var isDeleted = await repo.DeleteTempRequiredAsync(id);
            if (!isDeleted)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
    }
}
