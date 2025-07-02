using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pdftopng.Services;

namespace pdftopng.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PrintPdfFromHtmlController : ControllerBase
    {
        private readonly IHtmlToPdfService _HtmlToPdfService;

        public PrintPdfFromHtmlController(IHtmlToPdfService htmlToPdfService)
        {
            _HtmlToPdfService = htmlToPdfService;
        }

        [HttpPost]
        [Route("html-pdf")]
        public IActionResult PrintFromHtml([FromForm] TemplateRequest request)
        {
            // đang chạy version này
            try
            {
                _HtmlToPdfService.HtmlToPdfItext(html: request.Html, printerName: request.PrinterName, landscape: Convert.ToBoolean(request.Landscape), width: (int)request.Width, height: (int)request.Height);
                return StatusCode(200, "thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
        [HttpPost]
        [Route("html-pdf-v2")]
        public IActionResult PrintFromHtml_v2([FromForm] TemplateRequest request)
        {
            // đang chạy version này
            try
            {
                _HtmlToPdfService.HtmlToPdfItextv2(html: request.Html, printerName: request.PrinterName, landscape: Convert.ToBoolean(request.Landscape), width: (int)request.Width, height: (int)request.Height);
                return StatusCode(200, "thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
    }
    public sealed class TemplateRequest
    {
        [Required]
        public string Html { get; set; }

        [Required]
        public string PrinterName { get; set; }
        [Required]
        public string Landscape { get; set; }
        [Required]
        public int Width { get; set; }
        [Required]
        public int Height { get; set; }
    }
}
