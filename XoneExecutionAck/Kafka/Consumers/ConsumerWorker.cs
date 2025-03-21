// Individual consumer work unit (equivalent to ExecConsumer) 
using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using XoneExecutionAck.Kafka.Serialization;
using XoneExecutionAck.Processing;

namespace XoneExecutionAck.Kafka.Consumers
{
    public class ConsumerWorker
    {
        private readonly IConsumer<string, byte[]> _consumer;
        private readonly KafkaRecordDeserializer _deserializer;
        private readonly ExecutionProcessor _processor;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancellationToken;

        public ConsumerWorker(
            IConsumer<string, byte[]> consumer,
            KafkaRecordDeserializer deserializer,
            ExecutionProcessor processor,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            _consumer = consumer;
            _deserializer = deserializer;
            _processor = processor;
            _logger = logger;
            _cancellationToken = cancellationToken;
        }

        public async Task RunAsync()
        {
            try
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(_cancellationToken);
                        if (consumeResult == null) continue;

                        _logger.LogDebug("Consumed message at {Topic}:{Partition}:{Offset}",
                            consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);

                        // Deserialize and process the message
                        var record = _deserializer.Deserialize(consumeResult.Message.Value);

                        // Process the record
                        await _processor.ProcessAsync(record);

                        // Commit the offset after successful processing
                        _consumer.Commit(consumeResult);

                        _logger.LogInformation("Committed offset for {Topic}-{Partition} at offset {Offset}",
                            consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message");
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error during message consumption");
                    }
                }
            }
            finally
            {
                _consumer.Close();
            }
        }
    }
}