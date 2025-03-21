// Factory for creating Kafka consumers 
using System;
using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XoneExecutionAck.Configuration;

namespace XoneExecutionAck.Kafka.Consumers
{
    public class KafkaConsumerFactory
    {
        private readonly ILogger<KafkaConsumerFactory> _logger;
        private readonly AppConfig _config;

        public KafkaConsumerFactory(
            IOptions<AppConfig> config,
            ILogger<KafkaConsumerFactory> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public IConsumer<string, byte[]> CreateInfoConsumer()
        {
            var config = GetCommonConfig();
            config.GroupId = _config.Kafka.GroupId;
            return new ConsumerBuilder<string, byte[]>(config).Build();
        }

        public List<IConsumer<string, byte[]>> CreateEventConsumers()
        {
            switch (_config.Kafka.ConsumeMode.ToUpperInvariant())
            {
                case "AUTO":
                    using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _config.Kafka.BootstrapServers }).Build())
                    using (var consumer = CreateInfoConsumer())
                    {
                        var metadata = adminClient.GetMetadata(_config.Kafka.Topic, TimeSpan.FromSeconds(10));
                        var partitions = metadata.Topics
                            .FirstOrDefault(t => t.Topic == _config.Kafka.Topic)?.Partitions.Count ?? 1;

                        return Enumerable.Range(0, partitions)
                            .Select(CreateAutoConsumer)
                            .ToList();
                    }

                default:
                    // Default to AUTO with a single consumer
                    return new List<IConsumer<string, byte[]>> { CreateAutoConsumer(0) };
            }
        }

        private IConsumer<string, byte[]> CreateAutoConsumer(int index)
        {
            var config = GetCommonConfig();
            config.GroupId = _config.Kafka.GroupId;
            config.ClientId = $"{_config.Kafka.ClientId}{index}";
            config.AutoOffsetReset = (AutoOffsetReset)Enum.Parse(
                typeof(AutoOffsetReset),
                _config.Kafka.AutoOffsetReset,
                true);

            var consumer = new ConsumerBuilder<string, byte[]>(config).Build();
            consumer.Subscribe(_config.Kafka.Topic);

            return consumer;
        }

        private ConsumerConfig GetCommonConfig()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _config.Kafka.BootstrapServers,
                EnableAutoCommit = _config.Kafka.EnableAutoCommit,
                SessionTimeoutMs = _config.Kafka.SessionTimeoutMs,
                HeartbeatIntervalMs = _config.Kafka.HeartbeatIntervalMs,
                MaxPollIntervalMs = _config.Kafka.MaxPollIntervalMs,
            };

            if (_config.Kafka.SslEnabled)
            {
                config.SecurityProtocol = SecurityProtocol.Ssl;
                config.SslCaLocation = _config.Kafka.SslTruststorePath;
                config.SslCaPem = _config.Kafka.SslTruststorePassword;
            }

            return config;
        }
    }
}