
namespace file_conversion_api.Models
{
    public class ExceptionDetails
    {
        public int Status { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Detail { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
    }
}