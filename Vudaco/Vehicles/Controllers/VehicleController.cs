using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Shares.BaseRepository;
using Vudaco.Vehicles.Repositories;

namespace Vudaco.Vehicles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : BaseApiController
    {
        private readonly IVehicleRepositories _repoVehicle;
        private readonly IVehicleDispatchRepositories _repoVehicleDispatch;
        private readonly ILogger<VehicleController> _logger;
        private readonly VudacoDBContext _context;

        public VehicleController(ILogger<VehicleController> logger, IVehicleDispatchRepositories repoVehicleDispatch, IVehicleRepositories repoVehicle, VudacoDBContext context)
        {
            _logger = logger;
            _repoVehicle = repoVehicle;
            _repoVehicleDispatch = repoVehicleDispatch;
            _context = context;
        }
    }
}
