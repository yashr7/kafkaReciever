using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XoneExecutionAck.Database.Repositories;
using XoneExecutionAck.Kafka.Models;

namespace XoneExecutionAck.Processing
{
    public class ExecutionProcessor
    {
        private readonly ILogger<ExecutionProcessor> _logger;
        private readonly IExecutionRepository _repository;

        public ExecutionProcessor(
            ILogger<ExecutionProcessor> logger,
            IExecutionRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task ProcessAsync(KafkaRecord record)
        {
            try
            {
                _logger.LogDebug("Processing record: {Record}", record);

                // Save to database
                await _repository.SaveExecutionAsync(record);
                
                _logger.LogInformation("Successfully processed and saved execution record");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing execution record");
                throw; // Rethrow to trigger retry or error handling in consumer
            }
        }
    }
}