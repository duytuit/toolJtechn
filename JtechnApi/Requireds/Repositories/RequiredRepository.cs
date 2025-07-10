
using JtechnApi.Requireds.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using JtechnApi.Departments.Models;
using Microsoft.Extensions.Logging;
using JtechnApi.Employees.Repositories;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using JtechnApi.Shares.AdoHelper;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Dynamic;

namespace JtechnApi.Requireds.Repositories
{
    public class RequiredRepository : BaseRepository<Required>, IRequiredRepository
    {
        public const int from_dept = 1;
        public const int to_dept = 2;
        public const int from_type_rquired_accessory = 0;
        public const int from_type_check_machine_cut = 2;
        public const int from_type_check_device_v1 = 3;
        public const int from_type_check_device_v2 = 4;
        public const int from_type_rquired_assemble = 5;
        public const int from_type_cut_edp = 6;
        public const int from_type_vpp = 7;
        public const int from_type_task = 8;
        private readonly DBContext _context;
        private readonly ILogger<RequiredRepository> _logger;
        private readonly IEmployeeRepository _emp;
        private readonly RedisService _redis;
        private readonly IConfiguration _configuration;
        public RequiredRepository(DBContext context, ILogger<RequiredRepository> logger, IEmployeeRepository emp, RedisService redis, IConfiguration configuration) : base(context)
        {
            _context = context;
            _logger = logger;
            _emp = emp;
            _redis = redis;
            _configuration = configuration;
        }

        public async Task<PaginatedResult<Required>> GetPaginatedAsync(RequestRequiredDto RequestRequiredDto, int page, int pageSize)
        {
            // var totalItems = await _context.Required.CountAsync();
            var _query = _context.Required.AsQueryable();
            _query = _query.Where(u => u.Deleted_at == null);

            if (RequestRequiredDto.Status.HasValue)
            {
                _query = _query.Where(u => u.Status == RequestRequiredDto.Status);
            }
            if (RequestRequiredDto.From_type.HasValue)
            {
                _query = _query.Where(u => u.From_type == RequestRequiredDto.From_type);
            }
            int totalItems = await _query.CountAsync();
            var requireds = await _query.Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync();
            if (requireds == null || requireds.Count == 0)
            {
                return new PaginatedResult<Required>
                {
                    CurrentPage = page,
                    TotalPages = 0,
                    TotalItems = 0,
                    Items = new List<Required>()
                };
            }


            return new PaginatedResult<Required>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = requireds
            };
        }
        public async Task<PaginatedResult<Required>> GetTaskAsync(RequestRequiredDto RequestRequiredDto, int page, int pageSize)
        {
            var _query = _context.Required.AsQueryable();

            if (RequestRequiredDto.Status.HasValue)
            {
                _query = _query.Where(u => u.Status == RequestRequiredDto.Status);
            }
            if (RequestRequiredDto.Code != null && RequestRequiredDto.Code != "")
            {
                _query = _query.Where(u => u.Code == RequestRequiredDto.Code);
            }
            if (RequestRequiredDto.Title != null && RequestRequiredDto.Title != "")
            {
                _query = _query.Where(u => u.Title.Contains(RequestRequiredDto.Title));
            }
            if (RequestRequiredDto.Code_nv != null && RequestRequiredDto.Code_nv != "")
            {
                _query = _query.Where(u => u.Code.Contains(RequestRequiredDto.Code_nv));
            }
            if (RequestRequiredDto.From_type.HasValue)
            {
                _query = _query.Where(u => u.From_type == RequestRequiredDto.From_type);
            }
            if (RequestRequiredDto.Created_client.HasValue)
            {
                _query = _query.Where(u => u.Created_client == RequestRequiredDto.Created_client);
            }
            if (RequestRequiredDto.From_date.HasValue && RequestRequiredDto.To_date.HasValue)
            {
                _query = _query.WhereDateInRange(u => u.Created_at.Value, RequestRequiredDto.From_date.Value, RequestRequiredDto.To_date.Value);
            }
            int totalItems = await _query.CountAsync();
            var requireds = await _query.Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync();
            if (requireds == null || requireds.Count == 0)
            {
                return new PaginatedResult<Required>
                {
                    CurrentPage = page,
                    TotalPages = 0,
                    TotalItems = 0,
                    Items = new List<Required>()
                };
            }
            var signatureSubmissionIds = requireds.Select(c => c.Id).ToList();
            var signatureSubmissions = await _context.SignatureSubmission
                    .Where(p => signatureSubmissionIds.Contains(p.Required_id))
                    .AsNoTracking()
                    .ToListAsync();
            var departments = await _context.Department.AsNoTracking().Select($"new ({"id,code,name,status,permissions"})").ToDynamicListAsync();
            var result = new List<Required>();

            return new PaginatedResult<Required>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = result
            };
        }
        public async Task<PaginatedResult<object>> GetObjectTaskAsync(RequestRequiredDto dto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "id DESC"};
            if (dto.Status.HasValue)
                whereEquals["status"] = dto.Status.Value;

            if (!string.IsNullOrWhiteSpace(dto.Code))
                whereEquals[dto.Code.ToLower()] = dto.Code;

            if (!string.IsNullOrWhiteSpace(dto.Title))
                whereLikes["title"] = dto.Title;

            if (!string.IsNullOrWhiteSpace(dto.Code_nv))
                whereLikes["code"] = dto.Code_nv;

            if (dto.From_type.HasValue)
                whereEquals["from_type"] = dto.From_type.Value;

            if (dto.Created_client.HasValue)
                whereEquals["created_client"] = dto.Created_client.Value;

            if (dto.From_date.HasValue && dto.To_date.HasValue)
                whereDateRange.Add(("created_at", dto.From_date.Value, dto.To_date.Value));
            var departments = await _context.Department.AsNoTracking().Select($"new ({"id,code,name,status,permissions"})").ToDynamicListAsync();
            dynamic results = await AdoRelationQuery.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "requireds",
                        new[] { "id", "required_department_id" },
                        offset: 0,
                        limit: 10,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,      
                        relations: new List<AdoRelation>
                        {
                        new AdoRelation
                        {
                            Name = "signature_submissions",
                            Table = "signature_submissions",
                            Columns = new[] { "id", "required_id", "department_id" },
                            ParentKey = "id",
                            ForeignKey = "required_id",
                            KeyName = "required_id",
                            IsCollection = true,
                            SubRelations = new List<AdoRelation>
                            {
                                new AdoRelation
                                {
                                    Name = "department",
                                    Table = "departments",
                                    Columns = new[] { "id", "name" },
                                    ParentKey = "department_id",
                                    ForeignKey = "id",
                                    KeyName = "id",
                                    IsCollection = false
                                }
                            }
                        }
                        },
                        redisCache: _redis,
                        includeCount:true,
                        cancellationToken: cancellationToken
                    );
            int totalItems = results.Count;
            var objectList = new List<object>();
            objectList.AddRange(results.Data);
            var _results = new PaginatedResult<object>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = objectList,
            };
            // Gán thêm dữ liệu phụ
            _results.Extra["departments"] = departments;
            return _results;
        }

        public Task<Required> CreateRequiredAsync(Required required)
        {
            if (required == null)
            {
                throw new ArgumentNullException(nameof(required), "required cannot be null");
            }
            required.Created_at = DateTime.Now;
            required.Updated_at = DateTime.Now;
            _context.Required.Add(required);
            _context.SaveChanges();

            return Task.FromResult(required);
        }
        public Task<int> CheckDuplicateTitle(string title, int from_type, DateTime? created_client)
        {
            var _query = _context.Required.AsQueryable();
            _query = _query.Where(u => u.Deleted_at == null);
            _query = _query.Where(u => u.From_type == from_type);
            _query = _query.Where(u => u.Title == title);
            if (created_client.HasValue)
            {
                _query = _query.Where(u => u.Created_client == created_client);
            }
            var required = _query.CountAsync();
            return required;
        }
        public async Task<Required> show(int id)
        {
            Required required = await _context.Required.AsQueryable().Where(u => u.Id == id && u.Deleted_at == null).FirstOrDefaultAsync();
            if (required == null)
            {
                return null;
            }
            //required.Departments = await _context.Department.AsNoTracking().Select($"new ({"id,code,name,status,permissions"})").ToDynamicListAsync();
            return required;
        }
    }
}
