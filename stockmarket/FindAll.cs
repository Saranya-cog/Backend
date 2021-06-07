using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using stockmarket.Models;
using stockmarket.Service;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace stockmarket
{
    public  class FindAll
    {
        private readonly ILogger<FindAll> _logger;
        private readonly IStockService _stockService;
        private readonly IKafkaProducer _producer;

        public FindAll(
            ILogger<FindAll> logger,
            IStockService stockService,
            IKafkaProducer producer)
        {
            _logger = logger;
            _stockService = stockService;
            _producer = producer;
        }

        [FunctionName("FindAll")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ApiExplorerSettings(GroupName = "v1")]
        public  async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1.0/market/company/getall")] HttpRequest req,
             ILogger _logger)
        {
            try
            {
                var userDetails = await _stockService.GetUsers();

                if (userDetails.Count > 0)
                {
                    await _producer.SendEvent("stock-hubs",null,"Display All details from users list");
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(userDetails), Encoding.UTF8, "application/json")
                    };
                }

                await _producer.SendEvent("stock-hubs", null, "No Details are there to display");
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            }
            catch (Exception ex)
            {

                _logger.LogError($"Internal Server Error. Exception thrown: {ex.Message}");

                await _producer.SendEvent("stock-hubs", null, ex.Message);

                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

        }
    }
}
