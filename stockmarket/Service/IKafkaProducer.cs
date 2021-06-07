using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace stockmarket.Service
{
    public interface IKafkaProducer
    {
        Task SendEvent(string topicName, string key, string value);
    }
}
