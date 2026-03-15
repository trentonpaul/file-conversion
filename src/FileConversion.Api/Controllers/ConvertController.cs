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
        public async Task<IActionResult> Convert([FromQuery] string to)
        {
            var form = await Request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();

            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Code = "NoFile", Message = "No file uploaded." }
                });

            using var inputStream = new MemoryStream();
            await file.CopyToAsync(inputStream);
            inputStream.Position = 0;

            var job = await _jobService.SubmitJobAsync(inputStream, file.FileName, to);

            return Accepted(new ApiResponse<JobAcceptedDto>
            {
                Success = true,
                Data = new JobAcceptedDto { JobId = job.Id }
            });
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

            return Ok(new ApiResponse<JobStatusDto>
            {
                Success = true,
                Data = new JobStatusDto { Status = jobStatus.ToString() }
            });
        }

        [HttpGet("{id}/result")]
        public async Task<IActionResult> GetJobResult(Guid id)
        {
            var result = await _jobService.GetJobResultsAsync(id);

            if (!result.Exists)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Code = "NotFound", Message = $"Job with ID {id} not found." }
                });

            if (result.Status == JobStatus.Failed)
                return UnprocessableEntity(new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Code = "JobFailed", Message = result.ErrorMessage ?? "Job failed." }
                });

            if (result.Status != JobStatus.Complete || result.FileStream == null)
                return StatusCode(StatusCodes.Status409Conflict, new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Code = "NotReady", Message = $"Results for job {id} not ready. Status: {result.Status}" }
                });

            return File(result.FileStream, "application/octet-stream", result.FileName);
        }

        [HttpPost("batch")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> BatchConvert([FromForm] BatchConvertRequestDto request)
        {
            if (request.Files == null || request.Files.Count == 0)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Code = "NoFiles", Message = "No files uploaded." }
                });

            if (string.IsNullOrWhiteSpace(request.TargetFormat))
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Code = "NoTargetFormat", Message = "Target format is required." }
                });

            var jobs = new List<JobAcceptedDto>();

            foreach (var file in request.Files)
            {
                var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;
                var job = await _jobService.SubmitJobAsync(ms, file.FileName, request.TargetFormat);
                jobs.Add(new JobAcceptedDto { JobId = job.Id });
            }

            return Accepted(new ApiResponse<BatchConvertResponseDto>
            {
                Success = true,
                Data = new BatchConvertResponseDto { Jobs = jobs }
            });
        }

        [HttpPost("batch/status")]
        public async Task<IActionResult> BatchJobStatus([FromBody] BatchJobStatusRequestDto request)
        {
            var jobStatuses = await _jobService.GetJobStatusesAsync(request.JobIds);

            var statuses = request.JobIds.Zip(jobStatuses)
                .Select(pair => new JobStatusItemDto
                {
                    JobId = pair.First,
                    Status = pair.Second?.ToString() ?? "NotFound"
                })
                .ToList();

            return Ok(new ApiResponse<BatchJobStatusResponseDto>
            {
                Success = true,
                Data = new BatchJobStatusResponseDto { Statuses = statuses }
            });
        }

        [HttpPost("batch/results")]
        public async Task<IActionResult> BatchJobResults([FromBody] BatchJobResultRequestDto request)
        {
            var jobResults = await _jobService.GetJobResultsAsync(request.JobIds);

            var results = request.JobIds.Zip(jobResults)
                .Select(pair =>
                {
                    var (jobId, result) = pair;

                    if (!result.Exists)
                        return new JobResultItemDto { JobId = jobId, Status = "NotFound", Error = "Job not found." };

                    if (result.Status == JobStatus.Failed)
                        return new JobResultItemDto { JobId = jobId, Status = "Failed", Error = result.ErrorMessage ?? "Job failed." };

                    if (result.Status != JobStatus.Complete || result.FileStream == null)
                        return new JobResultItemDto { JobId = jobId, Status = result.Status?.ToString() ?? "Unknown", Error = "Job not ready." };

                    return new JobResultItemDto
                    {
                        JobId = jobId,
                        Status = "Complete",
                        DownloadUrl = Url.Action(nameof(GetJobResult), new { id = jobId })
                    };
                })
                .ToList();

            return Ok(new ApiResponse<BatchJobResultResponseDto>
            {
                Success = true,
                Data = new BatchJobResultResponseDto { Results = results }
            });
        }
    }
}