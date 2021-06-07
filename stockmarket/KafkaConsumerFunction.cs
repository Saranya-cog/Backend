using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using stockmarket.Service;

namespace stockmarket
{
    public  class KafkaConsumerFunction
    {
        private readonly IKafkaConsumer _consumer;

        public KafkaConsumerFunction(IKafkaConsumer consumer)
        {
            _consumer = consumer;
        }

        [FunctionName("KafkaConsumerFunction")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1.0/market/company/getConsumer")] HttpRequest req,
            ILogger log)
        {
            var res = await _consumer.getEvent("stock-hubs");

            return new OkObjectResult(res.Message.Value);
        }
    }
}
