
using JtechnApi.Accessorys.Models;
using JtechnApi.Requireds.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.Requireds.Repositories
{
    public class TempRequiredRepository : BaseRepository<TempRequired>, ITempRequiredRepository
    {
        private readonly OracleConnection _oracle;
        private readonly DBContext _context;
        public TempRequiredRepository(DBContext context,OracleConnection oracle) : base(context)
        {
            _context = context;
            _oracle = oracle;
        }
        public async Task<PaginatedResult<TempRequired>> GetPaginatedAsync(RequestRequiredDto RequestRequiredDto,int page, int pageSize)
        {
            var _query = _context.TempRequired.AsQueryable();
            if (!string.IsNullOrWhiteSpace(RequestRequiredDto.Keyword))
            {
               _query = _query.Where(c => c.Content.Contains(RequestRequiredDto.Keyword));
            }  
            if (!string.IsNullOrWhiteSpace(RequestRequiredDto.Code_nv))
            {
               _query = _query.Where(c => c.Code_nv.Contains(RequestRequiredDto.Code_nv));
            }   
            if (RequestRequiredDto.Created_at != null)
            {
                _query = _query.WhereDateInRange(u => u.Created_at.Value, RequestRequiredDto.Created_at.Value, RequestRequiredDto.Created_at.Value);
            }
            // check exits status empty 
            if (RequestRequiredDto.Status.HasValue)
            {   
                _query = _query.Where(u => u.Status == RequestRequiredDto.Status);
            }
            _query = _query.Where(u => u.Deleted_at == null);
            var lists = await _query.Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().OrderByDescending(x=>x.Created_at).ToListAsync();

            return new PaginatedResult<TempRequired>
            {
                //CurrentPage = page,
                //TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                //TotalItems = totalItems,
                Items = lists
            };
        }

        public Task<TempRequired> GetTempRequiredByIdAsync(int id)
        {
            var tempRequired = _context.TempRequired.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Deleted_at == null);
            if (tempRequired == null)
            {
                throw new KeyNotFoundException($"TempRequired with ID {id} not found.");
            }
            return tempRequired;
        }

        public Task<TempRequired> CreateTempRequiredAsync(TempRequired tempRequired)
        {
            if (tempRequired == null)
            {
                throw new ArgumentNullException(nameof(tempRequired), "TempRequired cannot be null");
            }
            tempRequired.Created_at = DateTime.Now;
            tempRequired.Updated_at = DateTime.Now;
            _context.TempRequired.Add(tempRequired);
            _context.SaveChanges();

            return Task.FromResult(tempRequired);
        }

        public Task<bool> DeleteTempRequiredAsync(int id)
        {
            var tempRequired = _context.TempRequired.Where(x => x.Id == id && x.Deleted_at == null).FirstOrDefault();
            if (tempRequired == null)
            {
                return Task.FromResult(false);
            }

            tempRequired.Deleted_at = DateTime.Now; // Soft delete
            _context.TempRequired.Update(tempRequired);
            _context.SaveChanges();

            return Task.FromResult(true);   
        }

        public Task<TempRequired> UpdateTempRequiredAsync(TempRequired tempRequired)
        {
            if (tempRequired == null)
            {
                throw new ArgumentNullException(nameof(tempRequired), "TempRequired cannot be null");
            }

            var existingTempRequired = _context.TempRequired.Find(tempRequired.Id);
            if (existingTempRequired == null)
            {
                throw new KeyNotFoundException($"TempRequired with ID {tempRequired.Id} not found.");
            }

            existingTempRequired.Msp = tempRequired.Msp;
            existingTempRequired.Code_nv = tempRequired.Code_nv;
            existingTempRequired.Content = tempRequired.Content;
            existingTempRequired.Status = tempRequired.Status;
            existingTempRequired.Updated_at = DateTime.Now;
            // Update other properties as needed
            _context.TempRequired.Update(existingTempRequired);
            _context.SaveChanges();
            return Task.FromResult(existingTempRequired);
        }
    }
}
