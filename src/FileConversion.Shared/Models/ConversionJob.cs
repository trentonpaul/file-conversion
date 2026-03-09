using System;
using System.Collections.Generic;
using System.Text;

namespace FileConversion.Shared.Models
{

    public class ConversionJob
    {
        public Guid Id { get; }
        public string OriginalFileName { get; }
        public string UploadFilePath { get; }
        public string TargetFormat { get; }
        public DateTime CreatedAt { get; }
        public JobStatus Status { get; set; }
        public string? OutputFilePath { get; set; }
        public string? ErrorMessage { get; private set; }

        private ConversionJob() { } // for EF only

        public ConversionJob(string originalFileName, string uploadFilePath, string targetFormat)
        {
            Id = Guid.NewGuid();
            OriginalFileName = originalFileName;
            UploadFilePath = uploadFilePath;
            TargetFormat = targetFormat;
            CreatedAt = DateTime.UtcNow;
            Status = JobStatus.Pending;
        }
    }

    public enum JobStatus
    {
        Pending,
        Processing,
        Complete,
        Failed
    }
}
