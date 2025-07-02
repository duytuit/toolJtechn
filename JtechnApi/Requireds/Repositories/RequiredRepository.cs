
using JtechnApi.Requireds.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.Requireds.Repositories
{
    public class RequiredRepository : BaseRepository<Required>, IRequiredRepository
    {
        private readonly DBContext _context;
        public RequiredRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<Required>> GetPaginatedAsync(int page, int pageSize)
        {
           // var totalItems = await _context.Required.CountAsync();
            var query_required = _context.Required.AsQueryable();
            //query_required.OrderBy(c => c.Id);
            query_required.Where(u => u.Deleted_at == null);
            var requireds = await query_required.Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync();
          
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
                required.Department = departments.Where(p => p.Id== required.Required_department_id).FirstOrDefault();
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
    }
}
