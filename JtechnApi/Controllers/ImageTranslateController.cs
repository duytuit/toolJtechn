using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Tesseract;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using JtechnApi.Shares;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageTranslateController: Controller
    {
        //private readonly ILogger<ImageTranslateController> _logger;

        private readonly DeepLSettings _deepLSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public ImageTranslateController(IOptions<DeepLSettings> deepLSettings, IHttpClientFactory httpClientFactory)
        {
            _deepLSettings = deepLSettings.Value;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string targetLang = "vi")
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var imageStream = file.OpenReadStream();
            using var image = SKBitmap.Decode(imageStream);
            if (image == null)
                return BadRequest("Invalid image file.");

            // OCR setup
            using var engine = new TesseractEngine(@"./tessdata", "jpn", EngineMode.Default);
            using var pix = Pix.LoadFromFile(file.FileName);
            using var page = engine.Process(pix);
            var iterator = page.GetIterator();

            iterator.Begin();

            var ocrResults = new List<(string text, int x, int y)>();

            do
            {
                if (iterator.IsAtBeginningOf(PageIteratorLevel.TextLine))
                {
                    var text = iterator.GetText(PageIteratorLevel.TextLine);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        iterator.TryGetBoundingBox(PageIteratorLevel.TextLine, out var rect);
                        ocrResults.Add((text.Trim(), rect.X1, rect.Y1));
                    }
                }
            } while (iterator.Next(PageIteratorLevel.TextLine));

            // Call DeepL API
            var translatedResults = new List<(string text, int x, int y)>();

            foreach (var item in ocrResults)
            {
                var translated = await TranslateDeepL(item.text, targetLang);
                translatedResults.Add((translated, item.x, item.y));
            }

            // Draw overlay
            using (var canvas = new SKCanvas(image))
            using (var paint = new SKPaint { Color = SKColors.Red })
            using (var typeface = SKTypeface.FromFamilyName("Arial"))
            using (var font = new SKFont(typeface, 24))
            {
                foreach (var item in translatedResults)
                    canvas.DrawText(item.text, item.x, item.y, SKTextAlign.Left, font, paint);
            }

            using var ms = new MemoryStream();
            using (var outputImage = SKImage.FromBitmap(image))
            using (var encodedImage = outputImage.Encode(SKEncodedImageFormat.Jpeg, 100))
            {
                if (encodedImage == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, "Could not encode image.");

                encodedImage.SaveTo(ms);
            }

            return File(ms.ToArray(), "image/jpeg");
        }

        private async Task<string> TranslateDeepL(string text, string targetLang)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var formData = new Dictionary<string, string>
        {
            { "auth_key", _deepLSettings.ApiKey },
            { "text", text },
            { "target_lang", targetLang.ToUpper() }
        };

            var response = await httpClient.PostAsync("https://api-free.deepl.com/v2/translate", new FormUrlEncodedContent(formData));

            if (!response.IsSuccessStatusCode)
                return text; // fallback

            var json = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(json);
            var result = doc.RootElement.GetProperty("translations")[0].GetProperty("text").GetString();

            return result ?? text;
        }
    }
}
