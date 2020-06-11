using AzureChallenge.Interfaces.Providers.Aggregates;
using AzureChallenge.Interfaces.Providers.Data;
using AzureChallenge.Models;
using AzureChallenge.Models.Aggregates;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Providers
{
    public class AggregateProvider : IAggregateProvider<AzureChallengeResult, Aggregate>
    {
        private IDataProvider<AzureChallengeResult, Aggregate> dataProvider;

        public AggregateProvider(IDataProvider<AzureChallengeResult, Aggregate> dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public async Task<AzureChallengeResult> AddItemAsync(Aggregate item)
        {
            return await dataProvider.UpsertItemAsync(item);
        }

        public async Task<(AzureChallengeResult, IList<Aggregate>)> GetAllItemsAsync()
        {
            return await dataProvider.GetAllItemsAsync("Aggregate");
        }

        public async Task<(AzureChallengeResult, Aggregate)> GetItemAsync(string id)
        {
            return await dataProvider.GetItemAsync(id, "Aggregate");
        }
    }
}
