using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using stockmarket.Models;
using stockmarket.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace stockmarket
{
    public  class CreateUser
    {   
        private readonly ILogger<CreateUser> _logger;
        private readonly IStockService _stockService;

        public CreateUser(
            ILogger<CreateUser> logger,
            IStockService stockService)
        {
            _logger = logger;
            _stockService = stockService;
        }

        [FunctionName(nameof(CreateUser))]
        [ProducesResponseType((int)StatusCodes.Status201Created)]
        [ApiExplorerSettings(GroupName = "v1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1.0/market/company/register")] HttpRequest req,ILogger _logger)
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

                    var userRequest = JsonConvert.DeserializeObject<User>(incomingRequest);

                    var userDetails = new User
                    {
                        _id = ObjectId.GenerateNewId().ToString(),
                        id = userRequest.id,
                        CompanyName = userRequest.CompanyName,
                        CompanyCode = userRequest.CompanyCode,
                        Turnover = userRequest.Turnover,
                        CompanyCeo = userRequest.CompanyCeo,
                        Website = userRequest.Website,
                        StockExchange = userRequest.StockExchange
                    };

                    if (userDetails.CompanyName == "" || userDetails.CompanyCode == "" || userDetails.StockExchange.Count == 0 || Int64.Parse(userDetails.Turnover) < 100000000 || userDetails.CompanyCeo == "" || userDetails.Website == "")
                    {
                        result = new StatusCodeResult(StatusCodes.Status406NotAcceptable);
                    }
                    else
                    {
                        var value = await _stockService.GetUsers();

                        User user = value.Find(x => x.CompanyCode == userRequest.CompanyCode);

                        if (user == null)
                        {
                            await _stockService.CreateUser(userDetails);
                            result = new StatusCodeResult(StatusCodes.Status201Created);
                        }
                        else
                        {
                            result = new StatusCodeResult(StatusCodes.Status302Found);
                        }
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
