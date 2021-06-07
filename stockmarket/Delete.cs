using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using stockmarket.Models;
using stockmarket.Service;
using System.Text;

namespace stockmarket
{
    public  class Delete
    {
        private readonly ILogger<Delete> _logger;
        private readonly IStockDetailService _stockDetailService;
        private readonly IStockService _stockService;
        private readonly IKafkaProducer _producer;

        public Delete(
            ILogger<Delete> logger,
            IStockDetailService stockDetailService, IStockService stockService, IKafkaProducer producer)
        {
            _logger = logger;
            _stockDetailService = stockDetailService;
            _stockService = stockService;
            _producer = producer;
        }

        [FunctionName("Delete")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ApiExplorerSettings(GroupName = "v1")]
        public  async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1.0/market/company/delete/{companyCode}")] HttpRequest req,
            ILogger _logger,string companyCode)
        {
            try
            {
                if (companyCode == "")
                {
                    await _producer.SendEvent("stock-hubs", null, "No Details to Delete");

                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }

                User res = await _stockService.GetUser(companyCode);

                if (res!=null)
                {
                    await _stockService.RemoveUserById(companyCode);

                    await _stockDetailService.RemoveStockById(companyCode);

                    await _producer.SendEvent("stock-hubs", null, "Delete All Details");

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("Deleted Successfully")
                    };

                }
                else
                {
                    await _producer.SendEvent("stock-hubs", null, "No Details to delete");
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }
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
