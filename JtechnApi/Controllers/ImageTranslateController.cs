using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Tesseract;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using JtechnApi.Shares;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

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

            // Load image
            using var image = Image.Load(file.OpenReadStream());

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
            var font = SystemFonts.CreateFont("Arial", 24);

            foreach (var item in translatedResults)
            {
                image.Mutate(ctx =>
                {
                    ctx.DrawText(item.text, font, Color.Red, new PointF(item.x, item.y));
                });
            }

            // Return image result
            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms);
            ms.Position = 0;

            return File(ms, "image/jpeg");
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
