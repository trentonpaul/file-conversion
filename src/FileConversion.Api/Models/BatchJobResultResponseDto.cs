using System;
using System.Collections.Generic;

namespace FileConversion.Api.Models
{
    public class BatchJobResultResponseDto
    {
        public List<JobResultItemDto> Results { get; set; } = new();
    }

    public class JobResultItemDto
    {
        public Guid JobId { get; set; }
        public string? DownloadUrl { get; set; }
        public string? Error { get; set; }
        public string? Status { get; set; }
    }
}