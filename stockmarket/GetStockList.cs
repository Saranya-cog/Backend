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
using stockmarket.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;

namespace stockmarket
{
    public class GetStockList
    {
        private readonly ILogger<GetStockList> _logger;
        private readonly IStockDetailService _stockDetailService;
        private readonly IKafkaProducer _producer;

        public GetStockList(
            ILogger<GetStockList> logger,
            IStockDetailService stockDetailService,
            IKafkaProducer producer)
        {
            _logger = logger;
            _stockDetailService = stockDetailService;
            _producer = producer;
        }

        [FunctionName(nameof(GetStockList))]
        [ProducesResponseType((int)StatusCodes.Status201Created)]
        [ApiExplorerSettings(GroupName = "v1")]
        public  async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get" , Route = "v1.0/market/stock/add/{code}/{startDate}/{endDate}")] HttpRequest req,string code,
            DateTime startDate,DateTime endDate,ILogger _logger)
        {
            try
            {
                List<stock> stockList = new List<stock>();

                List<stock> apiResults = await _stockDetailService.GetStocks(code);


                foreach (stock res in apiResults)
                {
                    if (res.Date>= startDate.Date && res.Date<= endDate.Date)
                    {
                        stockList.Add(res);
                    }
                }

                await _producer.SendEvent("stock-hubs", null, "Display All Stock Details");

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(stockList), Encoding.UTF8, "application/json")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal Server Error. Exception: {ex.Message}");

                await _producer.SendEvent("stock-hubs", null, ex.Message);

                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
