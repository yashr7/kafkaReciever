using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace XoneExecutionAck.Database.Entities
{
    public class ExecutionRecord
    {
        public long Id { get; set; }
        public required string ExecutionId { get; set; }
        public required string Symbol { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public required string Side { get; set; }

        // Use int to store Unix timestamp
        public int ExecutionTime { get; set; }

        public required string Market { get; set; }
        public required string RawData { get; set; }

        // Use int to store Unix timestamp
        public int CreatedAt { get; set; } = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Default to current Unix timestamp
    }
}