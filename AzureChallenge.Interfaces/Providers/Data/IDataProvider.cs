using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Interfaces.Providers.Data
{
    public interface IDataProvider<T, Q>
    {
        Task<(T, IEnumerable<Q>)> GetItemsAsync(string query);
        Task<(T, IList<Q>)> GetAllItemsAsync(string type);
        Task<(T, Q)> GetItemAsync(string id, string type);
        Task<T> UpsertItemAsync(Q item);
        Task<T> UpdateItemAsync(string id, Q item);
        Task<T> DeleteItemAsync(string id, string type);
    }
}
