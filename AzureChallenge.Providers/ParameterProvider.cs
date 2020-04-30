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
    public class ParameterProvider : IParameterProvider<AzureChallengeResult, GlobalParameters>
    {
        private IDataProvider<AzureChallengeResult, GlobalParameters> dataProvider;

        public ParameterProvider(IDataProvider<AzureChallengeResult, GlobalParameters> dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public async Task<AzureChallengeResult> AddItemAsync(GlobalParameters item)
        {
            return await dataProvider.AddItemAsync(item);
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
