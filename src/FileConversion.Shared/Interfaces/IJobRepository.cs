using FileConversion.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileConversion.Shared.Interfaces
{
    public interface IJobRepository
    {
        Task<Guid> CreateJobAsync(ConversionJob job);
        Task<ConversionJob> GetJobAsync(Guid jobId);
    }
}
