using AzureChallenge.Interfaces.Providers.Data;
using AzureChallenge.Models;
using AzureChallenge.Models.Questions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace AzureChallenge.Providers.DataProviders
{
    public class CosmosDbDataProvider : IDataProvider<AzureChallengeResult, Question>
    {
        private CosmosClient client;
        private readonly string databaseId;
        private readonly string containerId;

        public CosmosDbDataProvider(CosmosClient client, string databaseId, string containerId)
        {
            this.client = client;
            this.databaseId = databaseId;
            this.containerId = containerId;
        }

        public Task<(AzureChallengeResult, IEnumerable<Question>)> GetItemsAsync(string query)
        {
            throw new NotImplementedException();
        }

        public async Task<(AzureChallengeResult, Question)> GetItemAsync(string id)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            Question doc = null;

            try
            {
                ItemResponse<Question> response = await container.ReadItemAsync<Question>(id, new PartitionKey("Question"));

                doc = response.Resource;
                result.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                result.Success = false;
                result.Message = $"Failed to get question. No such document with id {id} found.";
            }

            return (result, doc);
        }

        public async Task<AzureChallengeResult> AddItemAsync(Question item)
        {
            var retVal = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                await container.CreateItemAsync<Question>(item, new PartitionKey(item.Type));
                retVal.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                retVal.Success = false;
                retVal.Message = "Failed to insert question into database. A document with the same Id already exists";
            }

            return retVal;
        }

        public async Task<AzureChallengeResult> UpdateItemAsync(string id, Question item)
        {
            throw new NotImplementedException();
        }

        public async Task<AzureChallengeResult> DeleteItemAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
