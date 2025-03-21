using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XoneExecutionAck.Database.Entities;
using XoneExecutionAck.Kafka.Models;

namespace XoneExecutionAck.Database.Repositories
{
    public interface IExecutionRepository
    {
        Task<ExecutionRecord> SaveExecutionAsync(KafkaRecord kafkaRecord);
        Task<bool> ExecutionExistsAsync(string executionId);
    }

    public class ExecutionRepository : IExecutionRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly ILogger<ExecutionRepository> _logger;

        // Add compiled queries for NativeAOT support
        private static readonly Func<AppDbContext, string, Task<bool>> CheckExecutionExistsQuery =
            EF.CompileAsyncQuery((AppDbContext context, string executionId) =>
                context.Executions.Any(e => e.ExecutionId == executionId));

        public ExecutionRepository(IDbContextFactory<AppDbContext> dbContextFactory, ILogger<ExecutionRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<ExecutionRecord> SaveExecutionAsync(KafkaRecord kafkaRecord)
        {
            try
            {
                // Extract execution ID from the record
                string executionId = ExtractExecutionId(kafkaRecord);

                // Check if execution already exists to avoid duplicates
                if (await ExecutionExistsAsync(executionId))
                {
                    _logger.LogInformation("Execution {ExecutionId} already exists, skipping", executionId);
                    return null;
                }

                // Map Kafka record to execution record
                var execution = MapToExecutionRecord(kafkaRecord);

                // Create a new DbContext for this operation
                using var context = await _dbContextFactory.CreateDbContextAsync();

                // Save to database
                context.Executions.Add(execution);
                await context.SaveChangesAsync();

                _logger.LogInformation("Saved execution {ExecutionId} to database", executionId);

                return execution;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving execution to database");
                throw;
            }
        }

        public async Task<bool> ExecutionExistsAsync(string executionId)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await CheckExecutionExistsQuery(context, executionId);
        }

        private ExecutionRecord MapToExecutionRecord(KafkaRecord kafkaRecord)
        {
            var execution = new ExecutionRecord
            {
                ExecutionId = ExtractExecutionId(kafkaRecord),
                Symbol = GetRequiredField(kafkaRecord, "symbol"),
                Side = GetRequiredField(kafkaRecord, "side"),
                Market = GetRequiredField(kafkaRecord, "market"),
                RawData = JsonSerializer.Serialize(kafkaRecord.Fields),
                CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() // Set CreatedAt to current Unix timestamp
            };

            if (kafkaRecord.Fields.TryGetValue("quantity", out object quantity))
                execution.Quantity = Convert.ToDecimal(quantity);

            if (kafkaRecord.Fields.TryGetValue("price", out object price))
                execution.Price = Convert.ToDecimal(price);

            if (kafkaRecord.Fields.TryGetValue("executionTime", out object execTime))
                execution.ExecutionTime = (int)DateTimeOffset.Parse(execTime.ToString()).ToUnixTimeSeconds(); // Convert to Unix timestamp
            else
                execution.ExecutionTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Default to current Unix timestamp

            return execution;
        }

        private string GetRequiredField(KafkaRecord record, string fieldName)
        {
            if (record.Fields.TryGetValue(fieldName, out object value) && value != null)
            {
                return value.ToString();
            }
            throw new ArgumentException($"Required field '{fieldName}' is missing in Kafka record");
        }

        private string ExtractExecutionId(KafkaRecord kafkaRecord)
        {
            // Implement logic to extract execution ID from the Kafka record
            return kafkaRecord.Fields["executionId"].ToString();
        }
    }
}