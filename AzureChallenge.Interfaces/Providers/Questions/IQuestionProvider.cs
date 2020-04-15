using System;
using System.Threading.Tasks;
using AzureChallenge.Models.Questions;

namespace AzureChallenge.Interfaces.Providers.Questions
{
    public interface IQuestionProvider<T, Q>
    {
        public Task<(T, Q)> GetItemAsync(string id);

        public Task<T> AddItemAsync(Q item);
    }
}
