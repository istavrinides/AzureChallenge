using AzureChallenge.Interfaces.Providers.Data;
using AzureChallenge.Interfaces.Providers.Tournaments;
using AzureChallenge.Models;
using AzureChallenge.Models.Tournaments;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Providers
{
    public class TournamentProvider : ITournamentProvider<AzureChallengeResult, TournamentDetails>
    {
        private IDataProvider<AzureChallengeResult, TournamentDetails> dataProvider;

        public TournamentProvider(IDataProvider<AzureChallengeResult, TournamentDetails> dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public async Task<AzureChallengeResult> AddItemAsync(TournamentDetails item)
        {
            return await dataProvider.AddItemAsync(item);
        }

        public async Task<(AzureChallengeResult, IList<TournamentDetails>)> GetAllItemsAsync()
        {
            return await dataProvider.GetAllItemsAsync("Tournament");
        }

        public async Task<(AzureChallengeResult, TournamentDetails)> GetItemAsync(string id)
        {
            return await dataProvider.GetItemAsync(id, "Tournament");
        }
    }
}
