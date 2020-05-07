using AzureChallenge.Interfaces.Providers.Data;
using AzureChallenge.Models;
using AzureChallenge.Models.Questions;
using AzureChallenge.Models.Tournaments;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using System.Net;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;
using AzureChallenge.Models.Parameters;

namespace AzureChallenge.Providers.DataProviders
{
    public class CosmosDbQuestionDataProvider: IDataProvider<AzureChallengeResult, Question>
    {
        private CosmosClient client;
        private readonly string databaseId;
        private readonly string containerId;

        public CosmosDbQuestionDataProvider(CosmosClient client, string databaseId, string containerId)
        {
            this.client = client;
            this.databaseId = databaseId;
            this.containerId = containerId;
        }

        public Task<(AzureChallengeResult, IEnumerable<Question>)> GetItemsAsync(string query)
        {
            throw new NotImplementedException();
        }

        public async Task<(AzureChallengeResult, IList<Question>)> GetAllItemsAsync(string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            QueryDefinition query = new QueryDefinition(
                "SELECT * FROM Questions q WHERE q.type = @Type")
                .WithParameter("@Type", type);

            FeedIterator<Question> resultSet = container.GetItemQueryIterator<Question>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(type),
                    MaxItemCount = 10
                });

            List<Question> allQuestions = new List<Question>();
            try
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<Question> response = await resultSet.ReadNextAsync();

                    foreach (var r in response)
                    {
                        allQuestions.Add(r);
                    }
                }
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = "Error while calling Cosmos Db";
            }

            result.Success = true;

            return (result, allQuestions);
        }

        public async Task<(AzureChallengeResult, Question)> GetItemAsync(string id, string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            Question doc = null;

            try
            {
                ItemResponse<Question> response = await container.ReadItemAsync<Question>(id, new PartitionKey(type));

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
                await container.UpsertItemAsync<Question>(item, new PartitionKey(item.Type));
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

        public async Task<AzureChallengeResult> DeleteItemAsync(string id, string type)
        {
            throw new NotImplementedException();
        }
    }

    public class CosmosDbTournamentDataProvider : IDataProvider<AzureChallengeResult, TournamentDetails>
    {
        private CosmosClient client;
        private readonly string databaseId;
        private readonly string containerId;

        public CosmosDbTournamentDataProvider(CosmosClient client, string databaseId, string containerId)
        {
            this.client = client;
            this.databaseId = databaseId;
            this.containerId = containerId;
        }

        public Task<(AzureChallengeResult, IEnumerable<TournamentDetails>)> GetItemsAsync(string query)
        {
            throw new NotImplementedException();
        }

        public async Task<(AzureChallengeResult, IList<TournamentDetails>)> GetAllItemsAsync(string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            QueryDefinition query = new QueryDefinition(
                "SELECT * FROM Tournaments t WHERE t.type = @Type")
                .WithParameter("@Type", type);

            FeedIterator<TournamentDetails> resultSet = container.GetItemQueryIterator<TournamentDetails>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(type),
                    MaxItemCount = 10
                });

            List<TournamentDetails> allQuestions = new List<TournamentDetails>();
            try
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<TournamentDetails> response = await resultSet.ReadNextAsync();

                    foreach (var r in response)
                    {
                        allQuestions.Add(r);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error while calling Cosmos Db";
            }

            result.Success = true;

            return (result, allQuestions);
        }

        public async Task<(AzureChallengeResult, TournamentDetails)> GetItemAsync(string id, string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            TournamentDetails doc = null;

            try
            {
                ItemResponse<TournamentDetails> response = await container.ReadItemAsync<TournamentDetails>(id, new PartitionKey(type));

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

        public async Task<AzureChallengeResult> AddItemAsync(TournamentDetails item)
        {
            var retVal = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                await container.UpsertItemAsync<TournamentDetails>(item, new PartitionKey(item.Type));
                retVal.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                retVal.Success = false;
                retVal.Message = "Failed to insert question into database. A document with the same Id already exists";
            }

            return retVal;
        }

        public async Task<AzureChallengeResult> UpdateItemAsync(string id, TournamentDetails item)
        {
            throw new NotImplementedException();
        }

        public async Task<AzureChallengeResult> DeleteItemAsync(string id, string type)
        {
            throw new NotImplementedException();
        }
    }

    public class CosmosDbParameterDataProvider : IDataProvider<AzureChallengeResult, GlobalParameters>
    {
        private CosmosClient client;
        private readonly string databaseId;
        private readonly string containerId;

        public CosmosDbParameterDataProvider(CosmosClient client, string databaseId, string containerId)
        {
            this.client = client;
            this.databaseId = databaseId;
            this.containerId = containerId;
        }

        public Task<(AzureChallengeResult, IEnumerable<GlobalParameters>)> GetItemsAsync(string query)
        {
            throw new NotImplementedException();
        }

        public async Task<(AzureChallengeResult, IList<GlobalParameters>)> GetAllItemsAsync(string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            QueryDefinition query = new QueryDefinition(
                "SELECT * FROM Tournaments t WHERE q.type = @Type")
                .WithParameter("@Type", type);

            FeedIterator<GlobalParameters> resultSet = container.GetItemQueryIterator<GlobalParameters>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(type),
                    MaxItemCount = 10
                });

            List<GlobalParameters> allQuestions = new List<GlobalParameters>();
            try
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<GlobalParameters> response = await resultSet.ReadNextAsync();

                    foreach (var r in response)
                    {
                        allQuestions.Add(r);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error while calling Cosmos Db";
            }

            result.Success = true;

            return (result, allQuestions);
        }

        public async Task<(AzureChallengeResult, GlobalParameters)> GetItemAsync(string id, string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            GlobalParameters doc = null;

            try
            {
                ItemResponse<GlobalParameters> response = await container.ReadItemAsync<GlobalParameters>(id, new PartitionKey(type));

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

        public async Task<AzureChallengeResult> AddItemAsync(GlobalParameters item)
        {
            var retVal = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                await container.UpsertItemAsync<GlobalParameters>(item, new PartitionKey(item.Type));
                retVal.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                retVal.Success = false;
                retVal.Message = "Failed to insert question into database. A document with the same Id already exists";
            }

            return retVal;
        }

        public async Task<AzureChallengeResult> UpdateItemAsync(string id, GlobalParameters item)
        {
            throw new NotImplementedException();
        }

        public async Task<AzureChallengeResult> DeleteItemAsync(string id, string type)
        {
            var retVal = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                ItemResponse<GlobalParameters> response = await container.DeleteItemAsync<GlobalParameters>(id: id, partitionKey: new PartitionKey(type));
                retVal.Success = response.StatusCode == HttpStatusCode.NoContent;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                retVal.Success = false;
                retVal.Message = "Failed to insert question into database. A document with the same Id already exists";
            }

            return retVal;
        }
    }


    public class CosmosDbAssignedQuestionDataProvider : IDataProvider<AzureChallengeResult, AssignedQuestion>
    {
        private CosmosClient client;
        private readonly string databaseId;
        private readonly string containerId;

        public CosmosDbAssignedQuestionDataProvider(CosmosClient client, string databaseId, string containerId)
        {
            this.client = client;
            this.databaseId = databaseId;
            this.containerId = containerId;
        }

        public Task<(AzureChallengeResult, IEnumerable<AssignedQuestion>)> GetItemsAsync(string query)
        {
            throw new NotImplementedException();
        }

        public async Task<(AzureChallengeResult, IList<AssignedQuestion>)> GetAllItemsAsync(string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            QueryDefinition query = new QueryDefinition(
                "SELECT * FROM Questions q WHERE q.type = @Type")
                .WithParameter("@Type", type);

            FeedIterator<AssignedQuestion> resultSet = container.GetItemQueryIterator<AssignedQuestion>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(type),
                    MaxItemCount = 10
                });

            List<AssignedQuestion> allQuestions = new List<AssignedQuestion>();
            try
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<AssignedQuestion> response = await resultSet.ReadNextAsync();

                    foreach (var r in response)
                    {
                        allQuestions.Add(r);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error while calling Cosmos Db";
            }

            result.Success = true;

            return (result, allQuestions);
        }

        public async Task<(AzureChallengeResult, AssignedQuestion)> GetItemAsync(string id, string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            AssignedQuestion doc = null;

            try
            {
                ItemResponse<AssignedQuestion> response = await container.ReadItemAsync<AssignedQuestion>(id, new PartitionKey(type));

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

        public async Task<AzureChallengeResult> AddItemAsync(AssignedQuestion item)
        {
            var retVal = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                await container.CreateItemAsync<AssignedQuestion>(item, new PartitionKey(item.Type));
                retVal.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                retVal.Success = false;
                retVal.Message = "Failed to insert question into database. A document with the same Id already exists";
            }

            return retVal;
        }

        public async Task<AzureChallengeResult> UpdateItemAsync(string id, AssignedQuestion item)
        {
            throw new NotImplementedException();
        }

        public async Task<AzureChallengeResult> DeleteItemAsync(string id, string type)
        {
            var retVal = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                ItemResponse<AssignedQuestion> response = await container.DeleteItemAsync<AssignedQuestion>(id: id, partitionKey: new PartitionKey(type));
                retVal.Success = response.StatusCode == HttpStatusCode.NoContent;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                retVal.Success = false;
                retVal.Message = "Failed to insert question into database. A document with the same Id already exists";
            }

            return retVal;
        }
    }
}
