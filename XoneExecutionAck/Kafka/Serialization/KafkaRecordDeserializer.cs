// Custom deserializer for Kafka records 
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using XoneExecutionAck.Kafka.Models;

namespace XoneExecutionAck.Kafka.Serialization
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(Dictionary<string, JsonElement>))]
    [JsonSerializable(typeof(Dictionary<string, object>))]
    [JsonSerializable(typeof(KafkaRecord))]
    internal partial class KafkaJsonContext : JsonSerializerContext
    {
        // The source generator will implement the required members
    }

    public class KafkaRecordDeserializer
    {
        private readonly ILogger<KafkaRecordDeserializer> _logger;
        private readonly JsonSerializerOptions _options;

        public KafkaRecordDeserializer(ILogger<KafkaRecordDeserializer> logger)
        {
            _logger = logger;
            // Use the default options from the generated context
            _options = KafkaJsonContext.Default.Options;
        }

        public KafkaRecord Deserialize(byte[] data)
        {
            var record = new KafkaRecord();

            try
            {
                if (data != null)
                {
                    var json = Encoding.UTF8.GetString(data);
                    // Use Dictionary<string, JsonElement> instead of Dictionary<string, object>
                    var fields = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, _options);

                    // Convert JsonElement values to objects
                    var convertedFields = new Dictionary<string, object>();
                    if (fields != null)
                    {
                        foreach (var kvp in fields)
                        {
                            convertedFields[kvp.Key] = ConvertJsonElement(kvp.Value);
                        }
                    }

                    record.Fields = convertedFields;
                    _logger.LogDebug("Read from Kafka: {Fields}", convertedFields);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to deserialize Kafka record");
            }

            return record;
        }

        private object ConvertJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                        return intValue;
                    if (element.TryGetInt64(out long longValue))
                        return longValue;
                    if (element.TryGetDouble(out double doubleValue))
                        return doubleValue;
                    return element.GetRawText();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                default:
                    return element.GetRawText();
            }
        }
    }
}