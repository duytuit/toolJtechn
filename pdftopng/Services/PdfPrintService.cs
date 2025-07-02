using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PdfiumViewer;
using static System.Net.Mime.MediaTypeNames;

namespace pdftopng.Services
{
    public class PdfPrintService : IPdfPrintService
    {
        private readonly ILogger<PdfPrintService> _logger;

        public PdfPrintService(ILogger<PdfPrintService> logger)
        {
            _logger = logger;
        }
        public void Print(Stream pdfStream, string printerName, string pageRange = null, int numberOfCopies = 1, int duplex = 1)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            using (var document = PdfDocument.Load(pdfStream))
            {
                _logger.LogInformation($"Printing PDF containing {document.PageCount} page(s) to printer '{printerName}'");
                using (var printDocument = document.CreatePrintDocument())
                {
                    // You can set printer settings if needed
                    printDocument.PrinterSettings = new System.Drawing.Printing.PrinterSettings()
                    {
                        PrinterName = printerName,
                        PrintRange = System.Drawing.Printing.PrintRange.AllPages
                    };

                    printDocument.Print();
                }
            }
        }
    }
    public interface IPdfPrintService
    {
        void Print(Stream pdfStream, string printerName, string pageRange = null, int numberOfCopies = 1, int duplex = 1);
    }
}
