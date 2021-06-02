using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using stockmarket.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stockmarket.Service
{
    public class StockService:IStockService
    {
            private readonly MongoClient _mongoClient;
            private readonly IMongoDatabase _database;
            private readonly IMongoCollection<User> _stock;

            public StockService(
                MongoClient mongoClient,
                IConfiguration configuration)
            {
                _mongoClient = mongoClient;
                _database = _mongoClient.GetDatabase("stockDB");
                _stock = _database.GetCollection<User>("userdetails");
            }

            public async Task CreateUser(User stockIn)
            {
                await _stock.InsertOneAsync(stockIn);
            }

            public async Task<User> GetUser(string code)
            {
                var book = await _stock.FindAsync(x => x.CompanyCode == code);
                return book.FirstOrDefault();
            }

            public async Task<List<User>> GetUsers()
            {
                var books = await _stock.FindAsync(x => true);
                return books.ToList();
            }
            public async Task RemoveUserById(string code)
            {
                 await _stock.DeleteOneAsync(x => x.CompanyCode == code);
            }

    }
}
