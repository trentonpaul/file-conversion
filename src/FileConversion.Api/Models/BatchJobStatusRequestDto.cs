using System;
using System.Collections.Generic;

namespace FileConversion.Api.Models
{
    public class BatchJobStatusRequestDto
    {
        public List<Guid> JobIds { get; set; } = new();
    }
}