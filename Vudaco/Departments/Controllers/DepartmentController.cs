using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Departments.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Departments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : BaseApiController
    {
        private readonly IDepartmentRepositories _repoDepartment;
        private readonly ILogger<DepartmentController> _logger;
        private readonly VudacoDBContext _context;

        public DepartmentController(ILogger<DepartmentController> logger, IDepartmentRepositories repoDepartment, VudacoDBContext context)
        {
            _logger = logger;
            _repoDepartment = repoDepartment;
            _context = context;
        }
    }
}
