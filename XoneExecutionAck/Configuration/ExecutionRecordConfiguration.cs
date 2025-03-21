using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XoneExecutionAck.Database.Entities;

namespace XoneExecutionAck.Database.Configurations
{
    public class ExecutionRecordConfiguration : IEntityTypeConfiguration<ExecutionRecord>
    {
        public void Configure(EntityTypeBuilder<ExecutionRecord> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.ExecutionId)
                .IsUnique();

            builder.Property(e => e.RawData)
                .HasColumnType("text"); // Change to text for string type

        }
    }
}
