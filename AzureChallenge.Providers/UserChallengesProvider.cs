using AzureChallenge.Interfaces.Providers.Data;
using AzureChallenge.Interfaces.Providers.Challenges;
using AzureChallenge.Models;
using AzureChallenge.Models.Challenges;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureChallenge.Interfaces.Providers.Users;
using AzureChallenge.Models.Users;

namespace AzureChallenge.Providers
{
    public class UserChallengesProvider : IUserChallengesProvider<AzureChallengeResult, UserChallenges>
    {
        private IDataProvider<AzureChallengeResult, UserChallenges> dataProvider;

        public UserChallengesProvider(IDataProvider<AzureChallengeResult, UserChallenges> dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public async Task<AzureChallengeResult> AddItemAsync(UserChallenges item)
        {
            return await dataProvider.UpsertItemAsync(item);
        }

        public async Task<(AzureChallengeResult, IList<UserChallenges>)> GetAllItemsAsync()
        {
            return await dataProvider.GetAllItemsAsync("UserChallenges");
        }

        public async Task<(AzureChallengeResult, UserChallenges)> GetItemAsync(string id)
        {
            return await dataProvider.GetItemAsync(id, "UserChallenges");
        }
    }
}
