using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Interfaces.Providers.Users
{
    public interface IUserChallengesProvider<T, Q>
    {
        public Task<(T, Q)> GetItemAsync(string id);

        public Task<(T, IList<Q>)> GetAllItemsAsync();

        public Task<T> AddItemAsync(Q item);

        public Task<T> DeleteItemAsync(string id);
    }
}
