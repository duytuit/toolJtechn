using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Employees.Models;
using Vudaco.Partners.Dtos;
using Vudaco.Partners.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Partners.Repositories
{
    public class PartnerDetailRepository : BaseRepository<PartnerDetail>, IPartnerDetailRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public PartnerDetailRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<PartnerDetail> CreateAsync(PartnerDetail PartnerDetail)
        {
            throw new NotImplementedException();
        }

        public Task<PartnerDetail> DeleteSoftAsync(PartnerDetail PartnerDetail)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(PartnerDetailDto PartnerDetailDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PartnerDetail> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<PartnerDetail> UpdateAsync(PartnerDetail PartnerDetail)
        {
            throw new NotImplementedException();
        }
    }
}
