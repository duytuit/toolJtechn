using System;
using System.IO;
using System.Threading.Tasks;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Http;

namespace pdftopng.Services
{
    public interface ICropPdfService
    {
        Task CropPdfPageAsync(IFormFile files, string outputPath, int x, int y, int with, int height);
        Task CropPdfPageAsyncv2(IFormFile files, string outputPath, int x, int y, int with, int height);
    }
    public class CropPdfService: ICropPdfService
    {
        public async Task CropPdfPageAsync(IFormFile formFile, string outputPath, int x, int y, int with, int height)
        {
            string path = Directory.GetCurrentDirectory();

            if (!Directory.Exists(path + "/image"))
                Directory.CreateDirectory(path + "/image");

            var uploadPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "image");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = System.IO.Path.Combine(uploadPath, formFile.FileName);

            string outputFilePath = outputPath == "/image/convert.pdf" ? path + "/image/convert.pdf" : outputPath;

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(stream);
            }
            try
            {
                PdfDocument pdfDoc = new PdfDocument(new PdfReader(filePath), new PdfWriter(outputFilePath));
            
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    PdfPage page = pdfDoc.GetPage(i);
                    Rectangle rect = new Rectangle(x, y, with, height);
                    page.SetMediaBox(rect);
                }
            
                pdfDoc.Close();
                File.Delete(filePath);
            }
            catch (Exception)
            {
                File.Delete(filePath);
            }
           
        }
        public async Task CropPdfPageAsyncv2(IFormFile formFile, string outputPath, int x, int y, int with, int height)
        {
            string path = Directory.GetCurrentDirectory();

            if (!Directory.Exists(path + "/image"))
                Directory.CreateDirectory(path + "/image");

            var uploadPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "image");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = System.IO.Path.Combine(uploadPath, formFile.FileName);

            string outputFilePath = outputPath == "/image/convert.pdf" ? path + "/image/convert.pdf" : outputPath;

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(stream);
            }
            try
            {
                using (Stream ms = new MemoryStream())
                {
                    PdfDocument pdfDoc = new PdfDocument(new PdfReader(ms), new PdfWriter(outputFilePath));

                    for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                    {
                        PdfPage page = pdfDoc.GetPage(i);
                        Rectangle rect = new Rectangle(x, y, with, height);
                        page.SetMediaBox(rect);
                    }

                    pdfDoc.Close();
                }
                  
                File.Delete(filePath);
            }
            catch (Exception)
            {
                File.Delete(filePath);
            }

        }
    }
}
