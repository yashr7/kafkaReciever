using Microsoft.EntityFrameworkCore;
using XoneExecutionAck.Database.Entities;

namespace XoneExecutionAck.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<ExecutionRecord> Executions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExecutionRecord>(entity =>
            {
                entity.Property(e => e.ExecutionTime)
                      .HasColumnType("integer"); // Ensure it's set to integer

                entity.Property(e => e.CreatedAt)
                      .HasColumnType("integer") // Ensure it's set to integer
                      .HasDefaultValueSql("EXTRACT(EPOCH FROM now())::integer"); // Default to current Unix timestamp
            });
        }
    }
}