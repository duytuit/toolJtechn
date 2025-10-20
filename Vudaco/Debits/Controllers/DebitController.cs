using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Debits.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Debits.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebitController : BaseApiController
    {
        private readonly IDebitRepositories _repoDebit;
        private readonly ILogger<DebitController> _logger;
        private readonly VudacoDBContext _context;

        public DebitController(ILogger<DebitController> logger, IDebitRepositories repoDebit, VudacoDBContext context)
        {
            _logger = logger;
            _repoDebit = repoDebit;
            _context = context;
        }
    }
}
