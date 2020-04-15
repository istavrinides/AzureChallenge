using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Interfaces.Providers.Data
{
    public interface IDataProvider<T, Q>
    {
        Task<(T, IEnumerable<Q>)> GetItemsAsync(string query);
        Task<(T, Q)> GetItemAsync(string id);
        Task<T> AddItemAsync(Q item);
        Task<T> UpdateItemAsync(string id, Q item);
        Task<T> DeleteItemAsync(string id);
    }
}
