using System;
using System.Threading.Tasks;
using AzureChallenge.Interfaces.Providers.Data;
using AzureChallenge.Interfaces.Providers.Questions;
using AzureChallenge.Models.Questions;
using AzureChallenge.Models;
using System.Collections.Generic;

namespace AzureChallenge.Providers
{
    public class AssignedQuestionProvider : IAssignedQuestionProvider<AzureChallengeResult, AssignedQuestion>
    {
        private IDataProvider<AzureChallengeResult, AssignedQuestion> dataProvider;

        public AssignedQuestionProvider(IDataProvider<AzureChallengeResult, AssignedQuestion> dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public async Task<(AzureChallengeResult, AssignedQuestion)> GetItemAsync(string id)
        {
            return await dataProvider.GetItemAsync(id , "AssignedQuestion");
        }

        public async Task<(AzureChallengeResult, IList<AssignedQuestion>)> GetAllItemsAsync()
        {
            return await dataProvider.GetAllItemsAsync("AssignedQuestion");
        }

        public async Task<AzureChallengeResult> AddItemAsync(AssignedQuestion item)
        {
            item.QuestionId = Guid.NewGuid().ToString();
            return await dataProvider.AddItemAsync(item);
        }
    }
}
