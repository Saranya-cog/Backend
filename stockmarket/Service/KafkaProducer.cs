using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace stockmarket.Service
{
    public class KafkaProducer:IKafkaProducer
    {
        private IProducer<string, string> _producer = null;

        public KafkaProducer(string brokerList,
            string connectionString,
            string caCertLocation)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = brokerList,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = "$ConnectionString",
                SaslPassword = connectionString,
               BrokerVersionFallback="0.10.0.0",
               ApiVersionFallbackMs=0,
               Debug= "security,broker,protocol"

            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public Task SendEvent(string topicName, string key, string value)
        {
            return _producer.ProduceAsync(topicName, new Message<string, string>
            {
                Key = key,
                Value = value
            });
        }

    }
}
