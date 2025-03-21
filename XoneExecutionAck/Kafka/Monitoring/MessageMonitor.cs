// Tracks message processing (similar to KafkaMessageMonitor) 
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using XoneExecutionAck.Kafka.Models;

namespace XoneExecutionAck.Kafka.Monitoring
{
    public class MessageMonitor
    {
        private readonly ILogger<MessageMonitor> _logger;
        private readonly ConcurrentDictionary<string, int> _topicPartitionMap = new ConcurrentDictionary<string, int>();
        private readonly ConcurrentDictionary<string, string> _messageMap = new ConcurrentDictionary<string, string>();

        public MessageMonitor(ILogger<MessageMonitor> logger)
        {
            _logger = logger;
        }

        public void Monitor(string pollKafkaKey, IReadOnlyList<KafkaRecord> records)
        {
            _logger.LogInformation("XONE monitor exec nbRecords: {Count}, pollKafkaKey: {Key}", 
                records.Count, pollKafkaKey);

            if (records == null || records.Count == 0)
            {
                return;
            }

            _topicPartitionMap[pollKafkaKey] = records.Count;

            foreach (var record in records)
            {
                string key;
                try
                {
                    // Equivalent to GusUtil.buildKeyKafka - implement your logic here
                    key = BuildKeyFromRecord(record, pollKafkaKey);

                    if (_messageMap.ContainsKey(key))
                    {
                        _logger.LogInformation("XONE contains duplicate executions for key={Key} and pollKafkaKey={PollKey}", 
                            key, pollKafkaKey);
                    }

                    _messageMap[key] = pollKafkaKey;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "XONE Unable to monitor exec");
                }
            }
        }

        public void FireProcessingDone(string messageKey, bool success)
        {
            try
            {
                if (!success) return;

                string pollKafkaKey = _messageMap.GetValueOrDefault(messageKey);
                if (pollKafkaKey == null) return;

                _logger.LogDebug("XONE remove monitor exec {Key} topic {PollKey}", 
                    messageKey, pollKafkaKey);

                if (_topicPartitionMap.TryGetValue(pollKafkaKey, out int count))
                {
                    int newCount = Interlocked.Decrement(ref count);
                    _topicPartitionMap[pollKafkaKey] = newCount;
                    
                    _logger.LogDebug("XONE topicPartitionMap {PollKey} value: {Value}", 
                        pollKafkaKey, newCount);

                    if (newCount == 0)
                    {
                        // All poll is done
                        _messageMap.TryRemove(messageKey, out _);
                        _logger.LogInformation("XONE monitor poll done pollKafkaKey: {PollKey}", 
                            pollKafkaKey);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "XONE message done error");
            }
        }

        public bool IsPollFinished(string pollKafkaKey)
        {
            return _topicPartitionMap.TryGetValue(pollKafkaKey, out int count) && count == 0;
        }

        public void Remove(string pollKafkaKey)
        {
            _topicPartitionMap.TryRemove(pollKafkaKey, out _);
        }

        // Helper method to build a unique key from a record
        private string BuildKeyFromRecord(KafkaRecord record, string pollKafkaKey)
        {
            // Implementation depends on your specific business logic
            // This should match the equivalent logic in GusUtil.buildKeyKafka
            
            if (record.Fields.TryGetValue("id", out object id))
            {
                return $"{id}_{pollKafkaKey}";
            }
            
            // Fallback to a unique identifier
            return $"{Guid.NewGuid()}_{pollKafkaKey}";
        }
    }
}