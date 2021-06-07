using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
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
    public  class FindCompany
    {
        private readonly ILogger<FindCompany> _logger;
        private readonly IStockService _stockService;
        private readonly IKafkaProducer _producer;

        public FindCompany(
            ILogger<FindCompany> logger,
            IStockService stockService,
            IKafkaProducer producer)
        {
            _logger = logger;
            _stockService = stockService;
            _producer = producer;
        }

        [FunctionName(nameof(FindCompany))]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [ApiExplorerSettings(GroupName = "v1")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1.0/market/company/info/{code}")] HttpRequest req,string code,ILogger _logger
         )
        {
            try
            {
                var userDetail = await _stockService.GetUser(code);

                if (userDetail!=null)
                {
                    await _producer.SendEvent("stock-hubs", null, "Display All details of specified company");
                    return  new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(userDetail), Encoding.UTF8, "application/json")
                    };
                }

                await _producer.SendEvent("stock-hubs", null, "Company Not Found");
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
