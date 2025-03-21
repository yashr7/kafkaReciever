// Model for Kafka record (similar to Java KafkaRecord) 
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XoneExecutionAck.Kafka.Models
{
    public class KafkaRecord
    {
        [JsonInclude]
        public Dictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();

        public override string ToString()
        {
            // Use the source-generated serializer from KafkaJsonContext
            return $"KafkaRecord [fields={JsonSerializer.Serialize(Fields, XoneExecutionAck.Kafka.Serialization.KafkaJsonContext.Default.Options)}]";
        }
    }
}