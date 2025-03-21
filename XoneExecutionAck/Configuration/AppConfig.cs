// Central configuration classes 
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using XoneExecutionAck.Kafka.Models;

namespace XoneExecutionAck.Configuration
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    public class AppConfig
    {
        public KafkaConfig Kafka { get; set; } = new KafkaConfig();
        public DatabaseConfig Database { get; set; } = new DatabaseConfig();
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    public class KafkaConfig
    {
        public string BootstrapServers { get; set; } = "localhost:29092";
        public string Topic { get; set; } = "gusack-topic";
        public string GroupId { get; set; } = "xone-execution-ack";
        public string ClientId { get; set; } = "xone-client";
        public string AutoOffsetReset { get; set; } = "earliest";
        public bool EnableAutoCommit { get; set; } = false;
        public int SessionTimeoutMs { get; set; } = 30000;
        public int HeartbeatIntervalMs { get; set; } = 10000;
        public int MaxPollIntervalMs { get; set; } = 300000;
        public int MaxPollRecords { get; set; } = 500;
        public string SecurityProtocol { get; set; } = "PLAINTEXT";
        public bool SslEnabled { get; set; } = false;
        public string SslTruststorePath { get; set; } = "";
        public string SslTruststorePassword { get; set; } = "";
        public string ConsumeMode { get; set; } = "Auto";
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    public class DatabaseConfig
    {
        public string ConnectionString { get; set; } = "Host=localhost;Database=xone_executions;Username=postgres;Password=postgres";
    }
}