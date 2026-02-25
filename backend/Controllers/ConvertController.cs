using Microsoft.AspNetCore.Mvc;
using ImageMagick;
using file_conversion_api.Services;
using Microsoft.AspNetCore.Http;

namespace file_conversion_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Post([FromQuery] string from, [FromQuery] string to, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using var input = new MemoryStream();
            await file.CopyToAsync(input);
            input.Position = 0;

            _logger.LogInformation("Received file of length {Length} bytes, converting from {From} to {To}", input.Length, from, to);

            var output = await _converter.ConvertAsync(input, from, to);

            _logger.LogInformation("Conversion complete, output stream length: {Length} bytes", output.Length);

            return File(output, $"image/{to?.ToLower()}", $"converted.{to?.ToLower()}");
        }
    }
}