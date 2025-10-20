using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Categorys.Dtos;
using Vudaco.Categorys.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Categorys.Repositories
{
    public class AdministrativeFeeRepository : BaseRepository<AdministrativeFeeCategory>, IAdministrativeFeeRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public AdministrativeFeeRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<AdministrativeFeeCategory> CreateAsync(AdministrativeFeeCategory AdministrativeFeeCategory)
        {
            throw new NotImplementedException();
        }

        public Task<AdministrativeFeeCategory> DeleteSoftAsync(AdministrativeFeeCategory AdministrativeFeeCategory)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(AdministrativeFeeCategoryDto AdministrativeFeeCategoryDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<AdministrativeFeeCategory> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<AdministrativeFeeCategory> UpdateAsync(AdministrativeFeeCategory AdministrativeFeeCategory)
        {
            throw new NotImplementedException();
        }
    }
}
