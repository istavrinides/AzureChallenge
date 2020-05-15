using System;
using System.Threading.Tasks;
using AzureChallenge.Interfaces.Providers.Data;
using AzureChallenge.Interfaces.Providers.Questions;
using AzureChallenge.Models.Questions;
using AzureChallenge.Models;
using System.Collections.Generic;

namespace AzureChallenge.Providers
{
    public class QuestionProvider : IQuestionProvider<AzureChallengeResult, Question>
    {
        private IDataProvider<AzureChallengeResult, Question> dataProvider;

        public QuestionProvider(IDataProvider<AzureChallengeResult, Question> dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public async Task<(AzureChallengeResult, Question)> GetItemAsync(string id)
        {
            return await dataProvider.GetItemAsync(id , "Question");
        }

        public async Task<(AzureChallengeResult, IList<Question>)> GetAllItemsAsync()
        {
            return await dataProvider.GetAllItemsAsync("Question");
        }

        public async Task<AzureChallengeResult> AddItemAsync(Question item)
        {
            return await dataProvider.AddItemAsync(item);
        }
    }
}
