using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pdftopng.Services;

namespace pdftopng.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PrinterController : ControllerBase
    {
        private readonly IPrinterService _printerService;

        public PrinterController(IPrinterService printerService)
        {
            _printerService = printerService;
        }

        [HttpGet]
        [Route("list")]
        public IActionResult ListPrinters()
        {
            string[] installedPrinters = _printerService.GetInstalledPrinters();
            return Ok(installedPrinters);
        }

        [HttpPost]
        [Route("install")]
        public IActionResult InstallPrinter([FromQuery] string printerPath)
        {
            _printerService.InstallPrinter(printerPath);
            return Ok();
        }
    }
}
