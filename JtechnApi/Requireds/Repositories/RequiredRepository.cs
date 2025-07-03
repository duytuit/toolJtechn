
using JtechnApi.Requireds.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<PaginatedResult<Required>> GetPaginatedAsync(RequestRequiredDto RequestRequiredDto,int page, int pageSize)
        {
            // var totalItems = await _context.Required.CountAsync();
            var _query = _context.Required.AsQueryable();
            _query.Where(u => u.Deleted_at == null);

            if (RequestRequiredDto.Status.HasValue)
            {   
                _query = _query.Where(u => u.Status == RequestRequiredDto.Status);
            }
            if (RequestRequiredDto.From_type.HasValue)
            {   
                _query = _query.Where(u => u.From_type == RequestRequiredDto.From_type);
            }

            var requireds = await _query.Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync();

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
                //CurrentPage = page,
                //TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                //TotalItems = totalItems,
                Items = result
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
        public Task<Required> CheckDuplicateTitle(string title, int from_type, DateTime? created_client)
        {
            var _query = _context.Required.AsQueryable();
            _query.Where(u => u.Deleted_at == null);
            _query = _query.Where(u => u.From_type == from_type);
            _query = _query.Where(u => u.Title == title);
            if (created_client.HasValue)
            {
                _query = _query.Where(u => u.Created_client == created_client.Value);
            }
            var required = _query.FirstOrDefaultAsync();
            return required;
        }
    }
}
