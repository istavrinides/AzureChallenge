using AzureChallenge.Interfaces.Providers.Data;
using AzureChallenge.Models;
using AzureChallenge.Models.Questions;
using AzureChallenge.Models.Challenges;
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
using AzureChallenge.Models.Aggregates;
using AzureChallenge.Models.Users;

namespace AzureChallenge.Providers.DataProviders
{
    public class CosmosDbQuestionDataProvider : IDataProvider<AzureChallengeResult, Question>
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

        public Task<(AzureChallengeResult, IList<Question>)> GetItemsAsync(string query, string type)
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

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = "Error while calling Cosmos Db";
            }

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
                result.IsError = false;
                result.Success = false;
                result.Message = $"Failed to get question. No such document with id {id} found.";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return (result, doc);
        }

        public async Task<AzureChallengeResult> UpsertItemAsync(Question item)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                await container.UpsertItemAsync<Question>(item, new PartitionKey(item.Type));
                result.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                result.IsError = false;
                result.Success = false;
                result.Message = "Failed to insert question into database. A document with the same Id already exists";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> UpdateItemAsync(string id, Question item)
        {
            throw new NotImplementedException();
        }

        public async Task<AzureChallengeResult> DeleteItemAsync(string id, string type)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                ItemResponse<GlobalChallengeParameters> response = await container.DeleteItemAsync<GlobalChallengeParameters>(id: id, partitionKey: new PartitionKey(type));
                result.Success = response.StatusCode == HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> DeleteItemsAsync(string query, string type)
        {
            throw new NotImplementedException();
        }
    }

    public class CosmosDbChallengeDataProvider : IDataProvider<AzureChallengeResult, ChallengeDetails>
    {
        private CosmosClient client;
        private readonly string databaseId;
        private readonly string containerId;

        public CosmosDbChallengeDataProvider(CosmosClient client, string databaseId, string containerId)
        {
            this.client = client;
            this.databaseId = databaseId;
            this.containerId = containerId;
        }

        public Task<(AzureChallengeResult, IList<ChallengeDetails>)> GetItemsAsync(string query, string type)
        {
            throw new NotImplementedException();
        }

        public async Task<(AzureChallengeResult, IList<ChallengeDetails>)> GetAllItemsAsync(string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            QueryDefinition query = new QueryDefinition(
                "SELECT * FROM Challenges c WHERE c.type = @Type")
                .WithParameter("@Type", type);

            FeedIterator<ChallengeDetails> resultSet = container.GetItemQueryIterator<ChallengeDetails>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(type),
                    MaxItemCount = 10
                });

            List<ChallengeDetails> allQuestions = new List<ChallengeDetails>();
            try
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<ChallengeDetails> response = await resultSet.ReadNextAsync();

                    foreach (var r in response)
                    {
                        allQuestions.Add(r);
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return (result, allQuestions);
        }

        public async Task<(AzureChallengeResult, ChallengeDetails)> GetItemAsync(string id, string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            ChallengeDetails doc = null;

            try
            {
                ItemResponse<ChallengeDetails> response = await container.ReadItemAsync<ChallengeDetails>(id, new PartitionKey(type));

                doc = response.Resource;
                result.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                result.IsError = false;
                result.Success = false;
                result.Message = $"Failed to get challenge. No such document with id {id} found.";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return (result, doc);
        }

        public async Task<AzureChallengeResult> UpsertItemAsync(ChallengeDetails item)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                await container.UpsertItemAsync<ChallengeDetails>(item, new PartitionKey(item.Type));
                result.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                result.Success = false;
                result.Message = "Failed to insert question into database. A document with the same Id already exists";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> UpdateItemAsync(string id, ChallengeDetails item)
        {
            throw new NotImplementedException();
        }

        public async Task<AzureChallengeResult> DeleteItemAsync(string id, string type)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                ItemResponse<GlobalChallengeParameters> response = await container.DeleteItemAsync<GlobalChallengeParameters>(id: id, partitionKey: new PartitionKey(type));
                result.Success = response.StatusCode == HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> DeleteItemsAsync(string query, string type)
        {
            throw new NotImplementedException();
        }
    }

    public class CosmosDbParameterDataProvider : IDataProvider<AzureChallengeResult, GlobalChallengeParameters>
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

        public Task<(AzureChallengeResult, IList<GlobalChallengeParameters>)> GetItemsAsync(string query, string type)
        {
            throw new NotImplementedException();
        }

        public async Task<(AzureChallengeResult, IList<GlobalChallengeParameters>)> GetAllItemsAsync(string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            QueryDefinition query = new QueryDefinition(
                "SELECT * FROM Parameters p WHERE p.type = @Type")
                .WithParameter("@Type", type);

            FeedIterator<GlobalChallengeParameters> resultSet = container.GetItemQueryIterator<GlobalChallengeParameters>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(type),
                    MaxItemCount = 10
                });

            List<GlobalChallengeParameters> allQuestions = new List<GlobalChallengeParameters>();
            try
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<GlobalChallengeParameters> response = await resultSet.ReadNextAsync();

                    foreach (var r in response)
                    {
                        allQuestions.Add(r);
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return (result, allQuestions);
        }

        public async Task<(AzureChallengeResult, GlobalChallengeParameters)> GetItemAsync(string id, string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            GlobalChallengeParameters doc = null;

            try
            {
                ItemResponse<GlobalChallengeParameters> response = await container.ReadItemAsync<GlobalChallengeParameters>(id, new PartitionKey(type));

                doc = response.Resource;
                result.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                result.IsError = false;
                result.Success = false;
                result.Message = $"Failed to get global challenge parameter. No such document with id {id} found.";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return (result, doc);
        }

        public async Task<AzureChallengeResult> UpsertItemAsync(GlobalChallengeParameters item)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                await container.UpsertItemAsync<GlobalChallengeParameters>(item, new PartitionKey(item.Type));
                result.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                result.Success = false;
                result.Message = "Failed to insert global challenge parameter into database. A document with the same Id already exists";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> UpdateItemAsync(string id, GlobalChallengeParameters item)
        {
            throw new NotImplementedException();
        }

        public async Task<AzureChallengeResult> DeleteItemAsync(string id, string type)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                ItemResponse<GlobalChallengeParameters> response = await container.DeleteItemAsync<GlobalChallengeParameters>(id: id, partitionKey: new PartitionKey(type));
                result.Success = response.StatusCode == HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> DeleteItemsAsync(string query, string type)
        {
            throw new NotImplementedException();
        }
    }

    public class CosmosDbGlobalParameterDataProvider : IDataProvider<AzureChallengeResult, GlobalParameters>
    {
        private CosmosClient client;
        private readonly string databaseId;
        private readonly string containerId;

        public CosmosDbGlobalParameterDataProvider(CosmosClient client, string databaseId, string containerId)
        {
            this.client = client;
            this.databaseId = databaseId;
            this.containerId = containerId;
        }

        public Task<(AzureChallengeResult, IList<GlobalParameters>)> GetItemsAsync(string query, string type)
        {
            throw new NotImplementedException();
        }

        public async Task<(AzureChallengeResult, IList<GlobalParameters>)> GetAllItemsAsync(string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            QueryDefinition query = new QueryDefinition(
                "SELECT * FROM GlobalParameters gp WHERE gp.type = @Type")
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

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

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
                result.IsError = false;
                result.Success = false;
                result.Message = $"Failed to get global parameter. No such document with id {id} found.";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return (result, doc);
        }

        public async Task<AzureChallengeResult> UpsertItemAsync(GlobalParameters item)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                await container.UpsertItemAsync<GlobalParameters>(item, new PartitionKey(item.Type));
                result.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                result.IsError = false;
                result.Success = false;
                result.Message = "Failed to insert global parameter into database. A document with the same Id already exists";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> UpdateItemAsync(string id, GlobalParameters item)
        {
            throw new NotImplementedException();
        }

        public async Task<AzureChallengeResult> DeleteItemAsync(string id, string type)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                ItemResponse<GlobalParameters> response = await container.DeleteItemAsync<GlobalParameters>(id: id, partitionKey: new PartitionKey(type));
                result.Success = response.StatusCode == HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> DeleteItemsAsync(string query, string type)
        {
            throw new NotImplementedException();
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

        public async Task<(AzureChallengeResult, IList<AssignedQuestion>)> GetItemsAsync(string queryText, string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            QueryDefinition query = new QueryDefinition(queryText);

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

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return (result, allQuestions);
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

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

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
                result.IsError = false;
                result.Success = false;
                result.Message = $"Failed to get assigned question. No such document with id {id} found.";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return (result, doc);
        }

        public async Task<AzureChallengeResult> UpsertItemAsync(AssignedQuestion item)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                await container.UpsertItemAsync<AssignedQuestion>(item, new PartitionKey(item.Type));
                result.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                result.IsError = false;
                result.Success = false;
                result.Message = "Failed to insert assigned question into database. A document with the same Id already exists";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> UpdateItemAsync(string id, AssignedQuestion item)
        {
            throw new NotImplementedException();
        }

        public async Task<AzureChallengeResult> DeleteItemAsync(string id, string type)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                ItemResponse<AssignedQuestion> response = await container.DeleteItemAsync<AssignedQuestion>(id: id, partitionKey: new PartitionKey(type));
                result.Success = response.StatusCode == HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> DeleteItemsAsync(string queryText, string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            QueryDefinition query = new QueryDefinition(queryText);

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
                        await container.DeleteItemAsync<AssignedQuestion>(id: r.QuestionId, partitionKey: new PartitionKey(r.Type));
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return (result);
        }
    }

    public class CosmosDbAggregateDataProvider : IDataProvider<AzureChallengeResult, Aggregate>
    {
        private CosmosClient client;
        private readonly string databaseId;
        private readonly string containerId;

        public CosmosDbAggregateDataProvider(CosmosClient client, string databaseId, string containerId)
        {
            this.client = client;
            this.databaseId = databaseId;
            this.containerId = containerId;
        }

        public Task<(AzureChallengeResult, IList<Aggregate>)> GetItemsAsync(string query, string type)
        {
            throw new NotImplementedException();
        }

        public async Task<(AzureChallengeResult, IList<Aggregate>)> GetAllItemsAsync(string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            QueryDefinition query = new QueryDefinition(
                "SELECT * FROM Questions q WHERE q.type = @Type")
                .WithParameter("@Type", type);

            FeedIterator<Aggregate> resultSet = container.GetItemQueryIterator<Aggregate>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(type),
                    MaxItemCount = 10
                });

            List<Aggregate> allQuestions = new List<Aggregate>();
            try
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<Aggregate> response = await resultSet.ReadNextAsync();

                    foreach (var r in response)
                    {
                        allQuestions.Add(r);
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return (result, allQuestions);
        }

        public async Task<(AzureChallengeResult, Aggregate)> GetItemAsync(string id, string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            Aggregate doc = null;

            try
            {
                ItemResponse<Aggregate> response = await container.ReadItemAsync<Aggregate>(id, new PartitionKey(type));

                doc = response.Resource;
                result.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                result.IsError = false;
                result.Success = false;
                result.Message = $"Failed to get aggregate. No such document with id {id} found.";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return (result, doc);
        }

        public async Task<AzureChallengeResult> UpsertItemAsync(Aggregate item)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                await container.UpsertItemAsync<Aggregate>(item, new PartitionKey(item.Type));
                result.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                result.IsError = false;
                result.Success = false;
                result.Message = "Failed to insert aggregate into database. A document with the same Id already exists";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> UpdateItemAsync(string id, Aggregate item)
        {
            throw new NotImplementedException();
        }

        public async Task<AzureChallengeResult> DeleteItemAsync(string id, string type)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                ItemResponse<Aggregate> response = await container.DeleteItemAsync<Aggregate>(id: id, partitionKey: new PartitionKey(type));
                result.Success = response.StatusCode == HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> DeleteItemsAsync(string query, string type)
        {
            throw new NotImplementedException();
        }
    }

    public class CosmosDbUserChallengesDataProvider : IDataProvider<AzureChallengeResult, UserChallenges>
    {
        private CosmosClient client;
        private readonly string databaseId;
        private readonly string containerId;

        public CosmosDbUserChallengesDataProvider(CosmosClient client, string databaseId, string containerId)
        {
            this.client = client;
            this.databaseId = databaseId;
            this.containerId = containerId;
        }

        public Task<(AzureChallengeResult, IList<UserChallenges>)> GetItemsAsync(string query, string type)
        {
            throw new NotImplementedException();
        }

        public async Task<(AzureChallengeResult, IList<UserChallenges>)> GetAllItemsAsync(string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            QueryDefinition query = new QueryDefinition(
                "SELECT * FROM UserChallenges uc WHERE uc.type = @Type")
                .WithParameter("@Type", type);

            FeedIterator<UserChallenges> resultSet = container.GetItemQueryIterator<UserChallenges>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(type),
                    MaxItemCount = 10
                });

            List<UserChallenges> allQuestions = new List<UserChallenges>();
            try
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<UserChallenges> response = await resultSet.ReadNextAsync();

                    foreach (var r in response)
                    {
                        allQuestions.Add(r);
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return (result, allQuestions);
        }

        public async Task<(AzureChallengeResult, UserChallenges)> GetItemAsync(string id, string type)
        {
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);
            var result = new AzureChallengeResult();
            UserChallenges doc = null;

            try
            {
                ItemResponse<UserChallenges> response = await container.ReadItemAsync<UserChallenges>(id, new PartitionKey(type));

                doc = response.Resource;
                result.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                result.IsError = false;
                result.Success = false;
                result.Message = $"Failed to get user aggregate. No such document with id {id} found.";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return (result, doc);
        }

        public async Task<AzureChallengeResult> UpsertItemAsync(UserChallenges item)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                await container.UpsertItemAsync<UserChallenges>(item, new PartitionKey(item.Type));
                result.Success = true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                result.IsError = false;
                result.Success = false;
                result.Message = "Failed to insert user aggregate into database. A document with the same Id already exists";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> UpdateItemAsync(string id, UserChallenges item)
        {
            throw new NotImplementedException();
        }

        public async Task<AzureChallengeResult> DeleteItemAsync(string id, string type)
        {
            var result = new AzureChallengeResult();
            var database = client.GetDatabase(databaseId);
            var container = database.GetContainer(containerId);

            try
            {
                ItemResponse<UserChallenges> response = await container.DeleteItemAsync<UserChallenges>(id: id, partitionKey: new PartitionKey(type));
                result.Success = response.StatusCode == HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        public async Task<AzureChallengeResult> DeleteItemsAsync(string query, string type)
        {
            throw new NotImplementedException();
        }
    }
}
