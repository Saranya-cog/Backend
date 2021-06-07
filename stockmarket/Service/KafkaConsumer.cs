using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace stockmarket.Service
{
    public class KafkaConsumer: IKafkaConsumer
    {
        private IConsumer<string, string> _consumer = null;

        public KafkaConsumer(string brokerList,string connectionString,string caCertLocation)
        {
            var config = new ConsumerConfig
            {
                GroupId = "$Default",
                BootstrapServers = brokerList,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = "$ConnectionString",
                SaslPassword = connectionString,
                BrokerVersionFallback = "0.10.0.0",
                ApiVersionFallbackMs = 0,
                Debug = "security,broker,protocol"

            };

            _consumer =new ConsumerBuilder<string, string>(config).SetKeyDeserializer(Deserializers.Utf8).SetValueDeserializer(Deserializers.Utf8).Build();
        }

        public async Task<ConsumeResult<string,string>> getEvent(string topicName)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
             _consumer.Subscribe(topicName);
            var consumeResult =_consumer.Consume(cts.Token);
            return consumeResult;
        }

    }
}
