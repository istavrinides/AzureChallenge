using AzureChallenge.Interfaces.Providers.Data;
using AzureChallenge.Interfaces.Providers.Parameters;
using AzureChallenge.Models;
using AzureChallenge.Models.Parameters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Providers
{
    public class ParameterProvider : IParameterProvider<AzureChallengeResult, GlobalChallengeParameters>
    {
        private IDataProvider<AzureChallengeResult, GlobalChallengeParameters> dataProvider;

        public ParameterProvider(IDataProvider<AzureChallengeResult, GlobalChallengeParameters> dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public async Task<AzureChallengeResult> AddItemAsync(GlobalChallengeParameters item)
        {
            return await dataProvider.UpsertItemAsync(item);
        }

        public async Task<AzureChallengeResult> DeleteItemAsync(string id)
        {
            return await dataProvider.DeleteItemAsync(id, "GlobalChallengeParameters");
        }

        public async Task<(AzureChallengeResult, IList<GlobalChallengeParameters>)> GetAllItemsAsync()
        {
            return await dataProvider.GetAllItemsAsync("GlobalChallengeParameters");
        }

        public async Task<(AzureChallengeResult, GlobalChallengeParameters)> GetItemAsync(string id)
        {
            return await dataProvider.GetItemAsync(id, "GlobalChallengeParameters");
        }
    }

    public class GlobalParameterProvider : IParameterProvider<AzureChallengeResult, GlobalParameters>
    {
        private IDataProvider<AzureChallengeResult, GlobalParameters> dataProvider;

        public GlobalParameterProvider(IDataProvider<AzureChallengeResult, GlobalParameters> dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public async Task<AzureChallengeResult> AddItemAsync(GlobalParameters item)
        {
            return await dataProvider.UpsertItemAsync(item);
        }

        public Task<AzureChallengeResult> DeleteItemAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<(AzureChallengeResult, IList<GlobalParameters>)> GetAllItemsAsync()
        {
            return await dataProvider.GetAllItemsAsync("GlobalParameters");
        }

        public async Task<(AzureChallengeResult, GlobalParameters)> GetItemAsync(string id)
        {
            return await dataProvider.GetItemAsync(id, "GlobalParameters");
        }
    }
}
