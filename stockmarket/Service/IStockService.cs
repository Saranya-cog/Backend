using stockmarket.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace stockmarket.Service
{
    public interface IStockService
    {
        
            Task<List<User>> GetUsers();

            Task<User> GetUser(string id);

            Task CreateUser(User userDetail);

            Task RemoveUserById(string id);

    }
}
