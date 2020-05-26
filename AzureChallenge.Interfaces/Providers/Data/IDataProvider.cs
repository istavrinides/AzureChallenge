using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Interfaces.Providers.Data
{
    public interface IDataProvider<T, Q>
    {
        Task<(T, IList<Q>)> GetItemsAsync(string query, string type);
        Task<(T, IList<Q>)> GetAllItemsAsync(string type);
        Task<(T, Q)> GetItemAsync(string id, string type);
        Task<T> UpsertItemAsync(Q item);
        Task<T> UpdateItemAsync(string id, Q item);
        Task<T> DeleteItemAsync(string id, string type);
        Task<T> DeleteItemsAsync(string query, string type);
    }
}
