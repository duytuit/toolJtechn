using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Partners.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Partners.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartnerController : BaseApiController
    {
        private readonly IPartnerDetailRepository _repoPartnerDetail;
        private readonly IPartnerRepository _repoPartner;
        private readonly ILogger<PartnerController> _logger;
        private readonly VudacoDBContext _context;

        public PartnerController(ILogger<PartnerController> logger, IPartnerRepository repoPartner, IPartnerDetailRepository repoPartnerDetail, VudacoDBContext context)
        {
            _logger = logger;
            _repoPartnerDetail = repoPartnerDetail;
            _repoPartner = repoPartner;
            _context = context;
        }
    }
}
