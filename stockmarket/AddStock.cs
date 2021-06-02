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
using MongoDB.Bson;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;

namespace stockmarket
{
    public  class AddStock
    {
        private readonly ILogger<AddStock> _logger;
        private readonly IStockDetailService _stockDetailService;

        public AddStock(
            ILogger<AddStock> logger,
            IStockDetailService stockDetailService)
        {
            _logger = logger;
            _stockDetailService = stockDetailService;
        }

        [FunctionName(nameof(AddStock))]
        [ProducesResponseType((int)StatusCodes.Status201Created)]
        [ApiExplorerSettings(GroupName = "v1")]
        public  async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1.0/market/stock/add/{companyCode}")] HttpRequest req,string companyCode,ILogger _logger)
        {
            IActionResult result;

            try
            {
                var headers = req.Headers;

                if (!headers.TryGetValue("clientId", out var clientId) ||
                   !headers.TryGetValue("clientSecret", out var clientSecret))
                {
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }

                var client_Id = clientId.First();

                var client_Secret = clientSecret.First();

                var httpClient = new HttpClient();

                var httpReq = new HttpRequestMessage(HttpMethod.Get, "https://login.microsoftonline.com/1b3d2e9b-794c-4ffe-8d86-5c870e3718ae/oauth2/v2.0/token");

                httpReq.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", client_Id.ToString() },
                    { "client_secret", client_Secret.ToString() },
                    { "scope", "api://8794bf5a-2f4a-46a3-acec-45b39a4d6982/.default" },
                });

                var resp = await httpClient.SendAsync(httpReq);

                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var incomingRequest = await new StreamReader(req.Body).ReadToEndAsync();

                    var stockReq = JsonConvert.DeserializeObject<stock>(incomingRequest);

                    var stockDetails = new stock
                    {
                        _id = ObjectId.GenerateNewId().ToString(),
                        StockPrice = stockReq.StockPrice,
                        CompanyCode = companyCode,
                        Date = DateTime.UtcNow.Date
                    };

                    if (stockReq.StockPrice == 0 || companyCode == "")
                    {
                        result = new StatusCodeResult(StatusCodes.Status406NotAcceptable);
                    }
                    else
                    {
                        await _stockDetailService.CreateStock(stockDetails);
                        result = new StatusCodeResult(StatusCodes.Status201Created);
                    }
                }
                else
                {
                    result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal Server Error. Exception: {ex.Message}");
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
