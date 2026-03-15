using System;
using System.Collections.Generic;

namespace FileConversion.Api.Models
{
    public class BatchConvertResponseDto
    {
        public List<JobAcceptedDto> Jobs { get; set; } = new();
    }
}