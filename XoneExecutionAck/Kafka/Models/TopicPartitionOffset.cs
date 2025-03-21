// Model for tracking partition offsets 
using Confluent.Kafka;

namespace XoneExecutionAck.Kafka.Models
{
    public class TopicPartitionOffset
    {
        public TopicPartition TopicPartition { get; }
        public long Offset { get; }
        public object DealMarketRef { get; }

        public TopicPartitionOffset(TopicPartition topicPartition, long offset)
        {
            TopicPartition = topicPartition;
            Offset = offset;
            DealMarketRef = string.Empty;
        }

        public TopicPartitionOffset(TopicPartition topicPartition, long offset, object dealMarketRef)
        {
            TopicPartition = topicPartition;
            Offset = offset;
            DealMarketRef = dealMarketRef ?? string.Empty;
        }

        public override string ToString()
        {
            return $"TopicPartitionOffset [topicPartition={TopicPartition}, offset={Offset}, dealMarketRef={DealMarketRef}]";
        }
    }
}