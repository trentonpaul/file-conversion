using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Immutable;
using FileConversion.Api.Interfaces;

namespace FileConversation.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ConvertController : ControllerBase
    {
        private readonly ILogger<ConvertController> _logger;
        private readonly IJobService _jobService;

        public ConvertController(ILogger<ConvertController> logger, IJobService jobService)
        {
            _logger = logger;
            _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Convert(
            [FromQuery] string to)
        {
            Stream inputStream;

            var form = await Request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");
    
            inputStream = new MemoryStream();
            await file.CopyToAsync(inputStream);
            inputStream.Position = 0;

            var fileName = file.FileName;

            // TODO: Queue conversion job to worker instead of converting directly
            // This could involve:
            // - Storing the uploaded file in blob storage
            // - Creating a job record in the database
            // - Publishing a message to a job queue (RabbitMQ, Azure Service Bus, etc.)
            // - Returning a job ID to the client

            var job = await _jobService.SubmitJobAsync(inputStream, fileName, to);
            
            return Accepted(new { message = $"File conversion job queued (Job ID: {job.Id}). The worker will process it." });
        }

    }
}