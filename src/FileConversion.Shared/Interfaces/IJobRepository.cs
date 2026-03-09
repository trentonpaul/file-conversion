using FileConversion.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileConversion.Shared.Interfaces
{
    public interface IJobRepository
    {
        Task<ConversionJob> CreateJobAsync(ConversionJob job);
        Task<ConversionJob?> GetJobAsync(Guid jobId);
        Task<ConversionJob> UpdateJobAsync(ConversionJob job);
    }
}
