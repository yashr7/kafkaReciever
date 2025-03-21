// Manages consumer lifecycle (renamed from KafkaConsumerManager for clarity) 
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XoneExecutionAck.Kafka.Serialization;
using XoneExecutionAck.Processing;

namespace XoneExecutionAck.Kafka.Consumers
{
    public class KafkaConsumerManager : BackgroundService
    {
        private readonly KafkaConsumerFactory _consumerFactory;
        private readonly KafkaRecordDeserializer _deserializer;
        private readonly ExecutionProcessor _processor;
        private readonly ILogger<KafkaConsumerManager> _logger;
        private readonly List<Task> _consumerTasks = new List<Task>();

        public KafkaConsumerManager(
            KafkaConsumerFactory consumerFactory,
            KafkaRecordDeserializer deserializer,
            ExecutionProcessor processor,
            ILogger<KafkaConsumerManager> logger)
        {
            _consumerFactory = consumerFactory;
            _deserializer = deserializer;
            _processor = processor;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("XONE KafkaConsumerManager starting");

            try
            {
                var consumers = _consumerFactory.CreateEventConsumers();

                foreach (var consumer in consumers)
                {
                    var worker = new ConsumerWorker(
                        consumer,
                        _deserializer,
                        _processor,
                        _logger,
                        stoppingToken
                    );

                    _consumerTasks.Add(worker.RunAsync());
                }

                await Task.WhenAll(_consumerTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Kafka consumers");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("XONE KafkaConsumerManager stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}
