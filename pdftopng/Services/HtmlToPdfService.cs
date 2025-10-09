using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Html2pdf;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Pdf;
using SelectPdf;
using VetCV.HtmlRendererCore.PdfSharpCore;

namespace pdftopng.Services
{
    public class HtmlToPdfService: IHtmlToPdfService
    {
        private readonly ILogger<HtmlToPdfService> _logger;

        public HtmlToPdfService(ILogger<HtmlToPdfService> logger)
        {
            _logger = logger;
        }

        public void HtmlToPdf(string html, string printerName)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (PdfSharpCore.Pdf.PdfDocument pdf = new PdfSharpCore.Pdf.PdfDocument())
                {
                    // Render the HTML content to the PDF
                    string testStylesheet = "<style> .product{ width: 350px; } div{ border: 1px solid black; } .product-code{ text-align: center; } .product-content{ display: flex; } .product-info{ width: 100%; display: flex; flex-direction: column; border: none; justify-content: space-around; } .product-info-item{ margin-left: 5px; border-bottom:none; border-top: none; border-left: none; border-right: none; } .product-qr{ border-top: none; border-left: none; border-bottom: none; } </style>";

                    var cssData =PdfGenerator.ParseStyleSheet(testStylesheet, true);

                    PdfGenerator.AddPdfPages(pdf, @html, PdfSharpCore.PageSize.A4, 20,cssData);
                        pdf.Save(ms);
                        using (var document = PdfiumViewer.PdfDocument.Load(ms))
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

        }
        public void HtmlToPdfItext(string html, string printerName, bool landscape, int width, int height)
        {
            string path = Directory.GetCurrentDirectory();

            if (!Directory.Exists(path + "/image"))
                Directory.CreateDirectory(path + "/image");

            var uploadPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "image");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = System.IO.Path.Combine(uploadPath, "output_with_css.pdf");

            // Create a PdfWriter instance
            using (PdfWriter writer = new PdfWriter(filePath))
            {
                // Initialize the PDF document
                using (iText.Kernel.Pdf.PdfDocument pdfDoc = new iText.Kernel.Pdf.PdfDocument(writer))
                {
                    // ConverterProperties to handle CSS and other resources
                    ConverterProperties converterProperties = new ConverterProperties();

                    // Convert the HTML string to PDF
                    HtmlConverter.ConvertToPdf(html, pdfDoc, converterProperties);
                }

            }
            using (var document = PdfiumViewer.PdfDocument.Load(filePath))
            {
                _logger.LogInformation($"Printing PDF containing {document.PageCount} page(s) to printer '{printerName}'");
                using (var printDocument = document.CreatePrintDocument())
                {
                  
                    int customWidth = width; // 8.5 inches * 100
                    int customHeight = height; // 5.5 inches * 100
                    PaperSize customPaperSize = new PaperSize("Custom Size", customWidth, customHeight);

                    // Assign the custom paper size to the PageSettings
                
                    PageSettings pageSettings = printDocument.DefaultPageSettings;
                    pageSettings.Landscape = landscape; // Set to true if landscape orientation is desired
                    pageSettings.Margins = new Margins(0, 0, 0, 0);
                    pageSettings.PrinterSettings.PrinterName = printerName;
                    pageSettings.PaperSize =  customPaperSize;
                    printDocument.Print();
                }
            }

        }
        public void HtmlToPdfItextv2(string html, string printerName, bool landscape, int width, int height)
        {
            string path = Directory.GetCurrentDirectory();

            if (!Directory.Exists(path + "/image"))
                Directory.CreateDirectory(path + "/image");

            var uploadPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "image");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePdf = System.IO.Path.Combine(uploadPath, "output_pdf.pdf");

            PdfPageSize pageSize = (PdfPageSize)Enum.Parse(typeof(PdfPageSize), "A4", true);

            PdfPageOrientation pdfOrientation = (PdfPageOrientation)Enum.Parse(typeof(PdfPageOrientation), "Portrait", true);

            // instantiate a html to pdf converter object
            HtmlToPdf converter = new HtmlToPdf();

            // set converter options
            converter.Options.PdfPageSize = pageSize;
            converter.Options.PdfPageOrientation = pdfOrientation;
            converter.Options.MarginLeft = 2;
            converter.Options.MarginRight = 5;
            // create a new pdf document converting an url
            SelectPdf.PdfDocument doc = converter.ConvertHtmlString(html);
            doc.Save(filePdf);
            doc.Close();
            using (var document = PdfiumViewer.PdfDocument.Load(filePdf))
            {
                _logger.LogInformation($"Printing PDF containing {document.PageCount} page(s) to printer '{printerName}'");
                using (var printDocument = document.CreatePrintDocument())
                {

                    int customWidth = width; // 8.5 inches * 100
                    int customHeight = height; // 5.5 inches * 100
                    PaperSize customPaperSize = new PaperSize("Custom Size", customWidth, customHeight);
                    // Assign the custom paper size to the PageSettings
                    PageSettings pageSettings = printDocument.DefaultPageSettings;
                    pageSettings.Landscape = landscape; // Set to true if landscape orientation is desired
                    pageSettings.Margins = new Margins(0, 0, 0, 0);
                    pageSettings.PrinterSettings.PrinterName = printerName;
                    pageSettings.PaperSize = customPaperSize;
                    printDocument.Print();
                }
            }

        }
    }
    public interface IHtmlToPdfService
    {
        void HtmlToPdf(string html, string printerName);
        void HtmlToPdfItext(string html, string printerName, bool landscape, int width, int height);
        void HtmlToPdfItextv2(string html, string printerName, bool landscape, int width, int height);
    }
}
