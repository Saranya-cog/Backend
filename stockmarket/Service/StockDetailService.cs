using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using stockmarket.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stockmarket.Service
{
    public class StockDetailService : IStockDetailService
    {
            private readonly MongoClient _mongoClient;
            private readonly IMongoDatabase _database;
            private readonly IMongoCollection<stock> _stocks;

            public StockDetailService(
                MongoClient mongoClient,
                IConfiguration configuration)
            {
                _mongoClient = mongoClient;
                _database = _mongoClient.GetDatabase("stockDB");
                _stocks = _database.GetCollection<stock>("stockdetails");
            }

            public async Task CreateStock(stock stockIn)
            {
                await _stocks.InsertOneAsync(stockIn);
            }

            public async Task<List<stock>> GetStocks(string code)
            {
                var stock = await _stocks.FindAsync(x => x.CompanyCode==code);
                return stock.ToList();
            }

            public async Task RemoveStockById(string code)
            {
              await _stocks.DeleteManyAsync(x => x.CompanyCode == code);
            }

    }
}
