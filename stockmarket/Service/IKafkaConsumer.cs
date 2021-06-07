using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace stockmarket.Service
{
    public interface IKafkaConsumer
    {
        Task<ConsumeResult<string, string>> getEvent(string topicName);
    }
}
