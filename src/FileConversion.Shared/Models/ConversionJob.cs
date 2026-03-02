using System;
using System.Collections.Generic;
using System.Text;

namespace FileConversion.Shared.Models
{
    public class ConversionJob
    {
        public Guid Id { get; }
        public Guid FileId { get; }
        public string TargetFormat { get; }
        public DateTime CreatedAt { get; }
        public JobStatus Status { get; private set; }
        public string? OutputFilePath { get; private set; }
        public string? ErrorMessage { get; private set; }

        public ConversionJob(Guid fileId, string targetFormat)
        {
            Id = Guid.NewGuid();
            FileId = fileId;
            TargetFormat = targetFormat;
            CreatedAt = DateTime.Now;
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
