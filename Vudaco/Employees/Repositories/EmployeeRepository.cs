using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Employees.Dtos;
using Vudaco.Employees.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Employees.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public EmployeeRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<Employee> CreateAsync(Employee Employee)
        {
            throw new NotImplementedException();
        }

        public Task<Employee> DeleteSoftAsync(Employee Employee)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(EmployeeDto EmployeeDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Employee> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Employee> UpdateAsync(Employee Employee)
        {
            throw new NotImplementedException();
        }
    }
}
