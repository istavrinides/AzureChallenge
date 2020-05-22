using AzureChallenge.Interfaces.Providers.Data;
using AzureChallenge.Interfaces.Providers.Challenges;
using AzureChallenge.Models;
using AzureChallenge.Models.Challenges;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Providers
{
    public class ChallengeProvider : IChallengeProvider<AzureChallengeResult, ChallengeDetails>
    {
        private IDataProvider<AzureChallengeResult, ChallengeDetails> dataProvider;

        public ChallengeProvider(IDataProvider<AzureChallengeResult, ChallengeDetails> dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public async Task<AzureChallengeResult> AddItemAsync(ChallengeDetails item)
        {
            return await dataProvider.UpsertItemAsync(item);
        }

        public async Task<(AzureChallengeResult, IList<ChallengeDetails>)> GetAllItemsAsync()
        {
            return await dataProvider.GetAllItemsAsync("Challenge");
        }

        public async Task<(AzureChallengeResult, ChallengeDetails)> GetItemAsync(string id)
        {
            return await dataProvider.GetItemAsync(id, "Challenge");
        }
    }
}
