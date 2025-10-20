using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Receipts.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Receipts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptController:BaseApiController
    {
        private readonly IReceiptDetailRepositories _repoReceiptDetail;
        private readonly IReceiptRepositories _repoReceipt;
        private readonly ILogger<ReceiptController> _logger;
        private readonly VudacoDBContext _context;

        public ReceiptController(ILogger<ReceiptController> logger, IReceiptDetailRepositories repoReceiptDetail, IReceiptRepositories repoReceipt, VudacoDBContext context)
        {
            _logger = logger;
            _repoReceiptDetail = repoReceiptDetail;
            _repoReceipt = repoReceipt;
            _context = context;
        }
    }
}
