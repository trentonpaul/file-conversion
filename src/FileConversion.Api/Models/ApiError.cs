using System.Collections.Generic;

namespace FileConversion.Api.Models
{
    public class ApiError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public IDictionary<string, string[]?>? Details { get; set; }
    }
}
