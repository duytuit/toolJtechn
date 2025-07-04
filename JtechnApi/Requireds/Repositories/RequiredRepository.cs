
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
        public RequiredRepository(DBContext context) : base(context)
        {
            _context = context;
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
            var signatureSubmissionIds = requireds.Select(c => c.Id).ToList();
            var employeeIds = requireds.Select(c => c.Created_by).ToList();
            var accessoryIds = requireds.Select(c => c.Code).ToList();
            var departmentIds = requireds.Select(c => c.Required_department_id).ToList();

            var signatureSubmissions = await _context.SignatureSubmission
                    .Where(p => signatureSubmissionIds.Contains(p.Id))
                    .AsNoTracking()
                    .ToListAsync();

            var employees = await _context.Employee
                   .Where(p => employeeIds.Contains(p.Id))
                   .AsNoTracking()
                   .ToListAsync();

            var accessorys = await _context.Accessory
                   .Where(p => accessoryIds.Contains(p.Code))
                   .AsNoTracking()
                   .ToListAsync();

            var departments = await _context.Department
                 .Where(p => departmentIds.Contains(p.Id))
                 .AsNoTracking()
                 .ToListAsync();

            var result = new List<Required>();

            foreach (var required in requireds)
            {
                required.SignatureSubmissions = signatureSubmissions.Where(p => p.Required_id == required.Id).ToList();
                required.Employee = employees.Where(p => p.Id == required.Created_by).FirstOrDefault();
                required.Accessory = accessorys.Where(p => p.Code == required.Code).FirstOrDefault();
                required.Department = departments.Where(p => p.Id == required.Required_department_id).FirstOrDefault();
                result.Add(required);
            }
            return new PaginatedResult<Required>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = result
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
            var employeeIds = requireds.Select(c => c.Created_by).ToList();
            var accessoryIds = requireds.Select(c => c.Code).ToList();
            var departmentIds = requireds.Select(c => c.Required_department_id).ToList();

            var signatureSubmissions = await _context.SignatureSubmission
                    .Where(p => signatureSubmissionIds.Contains(p.Id))
                    .AsNoTracking()
                    .ToListAsync();

            var employees = await _context.Employee
                   .Where(p => employeeIds.Contains(p.Id))
                   .AsNoTracking()
                   .ToListAsync();

            var accessorys = await _context.Accessory
                   .Where(p => accessoryIds.Contains(p.Code))
                   .AsNoTracking()
                   .ToListAsync();

            var departments = await _context.Department
                 .Where(p => departmentIds.Contains(p.Id))
                 .AsNoTracking()
                 .ToListAsync();

            var result = new List<Required>();

            foreach (var required in requireds)
            {
                required.SignatureSubmissions = signatureSubmissions.Where(p => p.Required_id == required.Id).ToList();
                required.Employee = employees.Where(p => p.Id == required.Created_by).FirstOrDefault();
                required.Accessory = accessorys.Where(p => p.Code == required.Code).FirstOrDefault();
                required.Department = departments.Where(p => p.Id == required.Required_department_id).FirstOrDefault();
                result.Add(required);
            }
            return new PaginatedResult<Required>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = result
            };
        }
        public async Task<PaginatedResult<object>> GetObjectTaskAsync(RequestRequiredDto dto, int page, int pageSize)
        {
            var query = _context.Required.AsQueryable();

            // Filtering conditions
            if (dto.Status.HasValue)
                query = query.Where(u => u.Status == dto.Status);

            if (!string.IsNullOrWhiteSpace(dto.Code))
                query = query.Where(u => u.Code == dto.Code);

            if (!string.IsNullOrWhiteSpace(dto.Title))
                query = query.Where(u => u.Title.Contains(dto.Title));

            if (!string.IsNullOrWhiteSpace(dto.Code_nv))
                query = query.Where(u => u.Code.Contains(dto.Code_nv));

            if (dto.From_type.HasValue)
                query = query.Where(u => u.From_type == dto.From_type);

            if (dto.Created_client.HasValue)
                query = query.Where(u => u.Created_client == dto.Created_client);

            if (dto.From_date.HasValue && dto.To_date.HasValue)
                query = query.WhereDateInRange(u => u.Created_at.Value, dto.From_date.Value, dto.To_date.Value);

            int totalItems = await query.CountAsync();

            // Projection with Fields (dynamic select)
            if (!string.IsNullOrWhiteSpace(dto.Fields))
            {
                var projection = $"new ({dto.Fields})";
                var dynamicQuery = query.Select(projection); // IQueryable<dynamic>

                var dynamicItems = await dynamicQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToDynamicListAsync();
                if (dynamicItems == null || dynamicItems.Count == 0)
                {
                    return new PaginatedResult<object>
                    {
                        CurrentPage = page,
                        TotalPages = 0,
                        TotalItems = 0,
                        Items = new List<object>()
                    };
                }
                return new PaginatedResult<object>
                {
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                    TotalItems = totalItems,
                    Items = dynamicItems.Cast<object>().ToList()
                };
            }

            // Nếu không chọn fields thì trả full entity và enrich
            var requireds = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            if (requireds == null || requireds.Count == 0)
            {
                return new PaginatedResult<object>
                {
                    CurrentPage = page,
                    TotalPages = 0,
                    TotalItems = 0,
                    Items = new List<object>()
                };
            }

            // Collect related IDs
            var requiredIds = requireds.Select(r => r.Id).ToList();
            var createdByIds = requireds.Select(r => r.Created_by).ToList();
            var codes = requireds.Select(r => r.Code).ToList();
            var departmentIds = requireds.Select(r => r.Required_department_id).ToList();

            // Load related entities
            var signatureSubmissions = await _context.SignatureSubmission
                .Where(s => requiredIds.Contains(s.Required_id))
                .AsNoTracking()
                .ToListAsync();

            var employees = await _context.Employee
                .Where(e => createdByIds.Contains(e.Id))
                .AsNoTracking()
                .ToListAsync();

            var accessories = await _context.Accessory
                .Where(a => codes.Contains(a.Code))
                .AsNoTracking()
                .ToListAsync();

            var departments = await _context.Department
                .Where(d => departmentIds.Contains(d.Id))
                .AsNoTracking()
                .ToListAsync();

            // Enrich entity
            foreach (var r in requireds)
            {
                r.SignatureSubmissions = signatureSubmissions.Where(s => s.Required_id == r.Id).ToList();
                r.Employee = employees.FirstOrDefault(e => e.Id == r.Created_by);
                r.Accessory = accessories.FirstOrDefault(a => a.Code == r.Code);
                r.Department = departments.FirstOrDefault(d => d.Id == r.Required_department_id);
            }

            return new PaginatedResult<object>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = requireds.Cast<object>().ToList()
            };
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
            required.SignatureSubmissions = await _context.SignatureSubmission.Where(p => p.Required_id == required.Id).AsNoTracking().ToListAsync();
            required.Employee = await _context.Employee.Where(p => p.Id == required.Created_by).AsNoTracking().FirstOrDefaultAsync();
            required.Accessory = await _context.Accessory.Where(p => p.Code == required.Code).AsNoTracking().FirstOrDefaultAsync();
            required.Department = await _context.Department.Where(p => p.Id == required.Required_department_id).AsNoTracking().FirstOrDefaultAsync();
            return required;
        }
    }
}
