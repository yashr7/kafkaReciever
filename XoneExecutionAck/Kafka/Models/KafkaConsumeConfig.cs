// Model for consumer configuration settings 
namespace XoneExecutionAck.Kafka.Models
{
    public class KafkaConsumeConfig
    {
        public string Topic { get; set; }
        public int Partition { get; set; }
        public long StartingOffset { get; set; }

        public override string ToString()
        {
            return $"KafkaConsumeConfig{{topic='{Topic}', partition={Partition}, startingOffset={StartingOffset}}}";
        }
    }
}