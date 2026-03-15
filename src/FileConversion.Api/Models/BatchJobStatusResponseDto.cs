using System;
using System.Collections.Generic;

namespace FileConversion.Api.Models
{
    public class BatchJobStatusResponseDto
    {
        public List<JobStatusItemDto> Statuses { get; set; } = new();
    }

    public class JobStatusItemDto
    {
        public Guid JobId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}