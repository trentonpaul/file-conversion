using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FileConversion.Api.Interfaces;
using FileConversion.Api.Models;
using FileConversion.Shared.Models;

namespace FileConversion.Api.Controllers
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
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Code = "NoFile", Message = "No file uploaded." }
                });

            inputStream = new MemoryStream();
            await file.CopyToAsync(inputStream);
            inputStream.Position = 0;

            var fileName = file.FileName;

            var job = await _jobService.SubmitJobAsync(inputStream, fileName, to);

            var payload = new JobAcceptedDto { JobId = job.Id };
            return Accepted(new ApiResponse<JobAcceptedDto> { Success = true, Data = payload });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobStatus(Guid id)
        {
            var jobStatus = await _jobService.GetJobStatusAsync(id);
            if (jobStatus == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Code = "NotFound", Message = $"Job with ID {id} not found." }
                });

            var payload = new JobStatusDto { Status = jobStatus.ToString() };
            return Ok(new ApiResponse<JobStatusDto> { Success = true, Data = payload });
        }

        [HttpGet("{id}/result")]
        public async Task<IActionResult> GetJobResult(Guid id)
        {
            var result = await _jobService.GetJobResultsAsync(id);
            if (!result.Exists)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Code = "NotFound", Message = $"Job with ID {id} not found." }
                });
            }
            if (result.Status == JobStatus.Failed)
            {
                return UnprocessableEntity(new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Code = "JobFailed", Message = result.ErrorMessage ?? "Job failed." }
                });
            }
            if (result.Status != JobStatus.Complete || result.FileStream == null)
            {
                return StatusCode(StatusCodes.Status409Conflict, new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Code = "NotReady", Message = $"Results for job {id} not ready. Status: {result.Status}" }
                });
            }
            return File(result.FileStream, "application/octet-stream", result.FileName);
        }

        [HttpPost("batch")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> BatchConvert([FromForm] BatchConvertRequestDto request)
        {
            if (request.Files == null || request.Files.Count == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Code = "NoFiles", Message = "No files uploaded." }
                });
            }
            if (request.TargetFormat == null || request.TargetFormat.Trim() == String.Empty)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Code = "NoTargetFormat", Message = "Target format is required." }
                });
            }
            var jobs = new List<JobAcceptedDto>();
            foreach (var file in request.Files)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;
                var job = await _jobService.SubmitJobAsync(ms, file.FileName, request.TargetFormat);
                jobs.Add(new JobAcceptedDto { JobId = job.Id });
            }
            return Accepted(new ApiResponse<BatchConvertResponseDto> { Success = true, Data = new BatchConvertResponseDto { Jobs = jobs } });
        }

        [HttpPost("batch/status")]
        public async Task<IActionResult> BatchJobStatus([FromBody] BatchJobStatusRequestDto request)
        {
            var statuses = new List<JobStatusItemDto>();
            var jobStatuses = await _jobService.GetJobStatusesAsync(request.JobIds);
            int i = 0;
            foreach (var jobId in request.JobIds)
            {
                var status = jobStatuses[i++];
                statuses.Add(new JobStatusItemDto { JobId = jobId, Status = status?.ToString() ?? "NotFound" });
            }
            return Ok(new ApiResponse<BatchJobStatusResponseDto> { Success = true, Data = new BatchJobStatusResponseDto { Statuses = statuses } });
        }

        [HttpPost("batch/results")]
        public async Task<IActionResult> BatchJobResults([FromBody] BatchJobResultRequestDto request)
        {
            var results = new List<JobResultItemDto>();
            var jobResults = await _jobService.GetJobResultsAsync(request.JobIds);
            int i = 0;
            foreach (var jobId in request.JobIds)
            {
                var result = jobResults[i++];
                if (!result.Exists)
                {
                    results.Add(new JobResultItemDto { JobId = jobId, Status = "NotFound", Error = "Job not found." });
                }
                else if (result.Status == JobStatus.Failed)
                {
                    results.Add(new JobResultItemDto { JobId = jobId, Status = "Failed", Error = result.ErrorMessage ?? "Job failed." });
                }
                else if (result.Status != JobStatus.Complete || result.FileStream == null)
                {
                    results.Add(new JobResultItemDto { JobId = jobId, Status = result.Status?.ToString() ?? "Unknown", Error = "Job not ready." });
                }
                else
                {
                    var url = Url.Action(nameof(GetJobResult), new { id = jobId });
                    results.Add(new JobResultItemDto { JobId = jobId, Status = "Complete", DownloadUrl = url });
                }
            }
            return Ok(new ApiResponse<BatchJobResultResponseDto> { Success = true, Data = new BatchJobResultResponseDto { Results = results } });
        }
    }
}
