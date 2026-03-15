using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace FileConversion.Api.Models
{
    public class BatchConvertRequestDto
    {
        public List<IFormFile> Files { get; set; } = new();
        public string TargetFormat { get; set; } = string.Empty;
    }
}