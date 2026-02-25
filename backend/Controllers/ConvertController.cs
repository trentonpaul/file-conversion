using Microsoft.AspNetCore.Mvc;
using ImageMagick;
using file_conversion_api.Services;
using Microsoft.AspNetCore.Http;

namespace file_conversion_api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ConvertController : ControllerBase
    {
        private readonly ILogger<ConvertController> _logger;
        private readonly IImageConverter _converter;

        public ConvertController(ILogger<ConvertController> logger, IImageConverter converter)
        {
            _logger = logger;
            _converter = converter;
        }

        [HttpPost]
        public async Task<IActionResult> Convert(
            [FromQuery] string to)
        {
            Stream inputStream;

            // Detect multipart/form-data
            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                var file = form.Files.FirstOrDefault();

                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");
    
                inputStream = new MemoryStream();
                await file.CopyToAsync(inputStream);
                inputStream.Position = 0;
            }
            else
            {
                // Raw binary upload
                inputStream = new MemoryStream();
                await Request.Body.CopyToAsync(inputStream);
                inputStream.Position = 0;

                if (inputStream.Length == 0)
                    return BadRequest("Empty request body.");
            }

            var output = await _converter.ConvertAsync(inputStream, to);

            return File(output, $"image/{to.ToLower()}", $"converted.{to.ToLower()}");
        }

    }
}