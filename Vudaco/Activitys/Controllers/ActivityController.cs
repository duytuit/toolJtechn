using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Activitys.Repositories;
using Vudaco.Controllers;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Activitys.Controllers
{
    public class ActivityController : BaseApiController
    {
        private readonly IActivityRepositories _repoActivity;
        private readonly ILogger<ActivityController> _logger;
        private readonly VudacoDBContext _context;

        public ActivityController(ILogger<ActivityController> logger, IActivityRepositories repoActivity, VudacoDBContext context)
        {
            _logger = logger;
            _repoActivity = repoActivity;
            _context = context;
        }
    }
}
