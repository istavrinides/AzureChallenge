using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Interfaces.Providers.Parameters
{
    public interface IParameterProvider<T, Q>
    {
        public Task<(T, Q)> GetItemAsync(string id);

        public Task<(T, IList<Q>)> GetAllItemsAsync();

        public Task<T> AddItemAsync(Q item);
    }
}
