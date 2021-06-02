using stockmarket.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stockmarket.Service
{
    public interface IStockDetailService
    {

        Task<List<stock>> GetStocks(string code);

        Task CreateStock(stock stockDetail);

        Task RemoveStockById(string id);

    }
}
