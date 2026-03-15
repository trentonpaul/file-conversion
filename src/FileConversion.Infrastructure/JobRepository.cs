using FileConversion.Shared.Interfaces;
using FileConversion.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FileConversion.Infrastructure
{
    public class JobRepository : IJobRepository
    {
        private readonly AppDbContext _context;

        public JobRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ConversionJob> CreateJobAsync(ConversionJob job)
        {
            await _context.Jobs.AddAsync(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<ConversionJob?> GetJobAsync(Guid jobId)
        {
            return await _context.Jobs.FindAsync(jobId);
        }

        public async Task<IReadOnlyList<ConversionJob?>> GetJobsAsync(IEnumerable<Guid> jobIds)
        {
            var idList = jobIds.ToList();
            var jobs = await _context.Jobs.Where(j => idList.Contains(j.Id)).ToListAsync();
            var jobDict = jobs.ToDictionary(j => j.Id, j => j);
            var result = new List<ConversionJob?>();
            foreach (var id in idList)
            {
                jobDict.TryGetValue(id, out var job);
                result.Add(job);
            }
            return result;
        }

        public async Task<ConversionJob> UpdateJobAsync(ConversionJob job)
        {
            _context.Jobs.Update(job);
            await _context.SaveChangesAsync();
            return job;
        }
    }
}
