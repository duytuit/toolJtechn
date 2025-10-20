using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Departments.Dtos;
using Vudaco.Departments.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Departments.Repositories
{
    public class DepartmentRepositories : BaseRepository<Department>, IDepartmentRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public DepartmentRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Department> CreateAsync(Department Department)
        {
            throw new NotImplementedException();
        }

        public Task<Department> DeleteSoftAsync(Department Department)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(DepartmentDto DepartmentDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Department> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Department> UpdateAsync(Department Department)
        {
            throw new NotImplementedException();
        }
    }
}
