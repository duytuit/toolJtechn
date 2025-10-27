using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Employees.Dtos;
using Vudaco.Employees.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Employees.Repositories
{
    public class EmployeeDepartmentRepository : BaseRepository<EmployeeDepartment>, IEmployeeDepartmentRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public EmployeeDepartmentRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<EmployeeDepartment> CreateAsync(EmployeeDepartment EmployeeDepartment)
        {
            throw new NotImplementedException();
        }

        public Task<EmployeeDepartment> DeleteSoftAsync(EmployeeDepartment EmployeeDepartment)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(EmployeeDepartmentDto EmployeeDepartmentDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<EmployeeDepartment> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<EmployeeDepartment> UpdateAsync(EmployeeDepartment EmployeeDepartment)
        {
            throw new NotImplementedException();
        }
    }
}
