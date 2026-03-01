using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace FileConversation.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ConvertController : ControllerBase
    {
        private readonly ILogger<ConvertController> _logger;

        public ConvertController(ILogger<ConvertController> logger)
        {
            _logger = logger;
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

            // TODO: Queue conversion job to worker instead of converting directly
            // This could involve:
            // - Storing the uploaded file in blob storage
            // - Creating a job record in the database
            // - Publishing a message to a job queue (RabbitMQ, Azure Service Bus, etc.)
            // - Returning a job ID to the client
            
            return Accepted(new { message = "File conversion job queued. The worker will process it." });
        }

    }
}