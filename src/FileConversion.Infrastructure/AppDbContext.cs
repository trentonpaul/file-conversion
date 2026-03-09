using FileConversion.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileConversion.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ConversionJob> Jobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConversionJob>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.OriginalFileName).IsRequired();
                entity.Property(x => x.UploadFilePath).IsRequired();
                entity.Property(x => x.TargetFormat).IsRequired();
                entity.Property(x => x.Status)
                    .HasConversion<string>();
                entity.Property(x => x.CreatedAt).IsRequired();
                entity.Property(x => x.OutputFilePath);
                entity.Property(x => x.ErrorMessage);
            });
        }
    }
}