using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Bills.Repositories;
using Vudaco.Controllers;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Bills.Controllers
{
    public class BillController : BaseApiController
    {
        private readonly IBillRepositories _repoBill;
        private readonly ILogger<BillController> _logger;
        private readonly VudacoDBContext _context;

        public BillController(ILogger<BillController> logger, IBillRepositories repoBill, VudacoDBContext context)
        {
            _logger = logger;
            _repoBill = repoBill;
            _context = context;
        }
    }
}
