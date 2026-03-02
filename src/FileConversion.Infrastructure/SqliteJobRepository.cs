using FileConversion.Shared.Interfaces;
using FileConversion.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileConversion.Infrastructure
{
    public class SqliteJobRepository : IJobRepository
    {
        public Task<Guid> CreateJobAsync(ConversionJob job)
        {
            throw new NotImplementedException();
        }

        public Task<ConversionJob> GetJobAsync(Guid jobId)
        {
            throw new NotImplementedException();
        }
    }
}
