using System;
using System.Threading.Tasks;
using AzureChallenge.Interfaces.Providers.Data;
using AzureChallenge.Interfaces.Providers.Questions;
using AzureChallenge.Models.Questions;
using AzureChallenge.Models;
using System.Collections.Generic;
using AzureChallenge.Interfaces.Providers.REST;
using Newtonsoft.Json.Linq;
using Microsoft.VisualBasic.CompilerServices;
using System.Threading.Tasks.Sources;
using System.Linq;
using AzureChallenge.Interfaces.Providers.Parameters;
using ACM = AzureChallenge.Models;
using ACMP = AzureChallenge.Models.Parameters;
using AzureChallenge.Models.Profile;
using System.Xml.Linq;

namespace AzureChallenge.Providers
{
    public class AssignedQuestionProvider : IAssignedQuestionProvider<AzureChallengeResult, AssignedQuestion>
    {
        private readonly IDataProvider<AzureChallengeResult, AssignedQuestion> dataProvider;
        private readonly IAzureAuthProvider authProvider;
        private readonly IRESTProvider restProvider;
        private readonly IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalTournamentParameters> parametersProvider;

        public AssignedQuestionProvider(IDataProvider<AzureChallengeResult, AssignedQuestion> dataProvider,
                                        IAzureAuthProvider authProvider,
                                        IRESTProvider restProvider,
                                        IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalTournamentParameters> parametersProvider)
        {
            this.dataProvider = dataProvider;
            this.authProvider = authProvider;
            this.restProvider = restProvider;
            this.parametersProvider = parametersProvider;
        }

        public async Task<(AzureChallengeResult, AssignedQuestion)> GetItemAsync(string id)
        {
            return await dataProvider.GetItemAsync(id, "AssignedQuestion");
        }

        public async Task<(AzureChallengeResult, IList<AssignedQuestion>)> GetAllItemsAsync()
        {
            return await dataProvider.GetAllItemsAsync("AssignedQuestion");
        }

        public async Task<AzureChallengeResult> AddItemAsync(AssignedQuestion item)
        {
            return await dataProvider.AddItemAsync(item);
        }

        public async Task<AzureChallengeResult> DeleteItemAsync(string id)
        {
            return await dataProvider.DeleteItemAsync(id, "AssignedQuestion");
        }

        public async Task<List<KeyValuePair<string, bool>>> ValidateQuestion(string id, UserProfile profile)
        {
            // Get the question definition
            var result = await GetItemAsync(id);
            // Create a list to check validity of answers
            List<KeyValuePair<string, bool>> correctAsnwers = new List<KeyValuePair<string, bool>>();

            if (!result.Item1.Success)
                return null;

            var question = result.Item2;

            // Get the global parameters
            var globalParameters = await parametersProvider.GetItemAsync(question.TournamentId);

            List<KeyValuePair<string, string>> additionalHeaders = null;

            for (int i = 0; i < question.Uris.Count; i++)
            {
                var parameters = new Dictionary<string, string>();

                // Global parameters don't have the Global. prefix, so create a new dictionary with that as the key name
                foreach (var gp in globalParameters.Item2.Parameters)
                {
                    parameters.Add($"Global_{gp.Key}", gp.Value);
                }
                if (question.Uris[i].CallType == "GET")
                {
                    // Prepare the uri
                    // First we need to replace the . notation for Global and Profile placeholders to _ (SmartFormat doesn't like the . notation)
                    var formattedUri = question.Uris[i].Uri.Replace("Global.", "Global_").Replace("Profile.", "Profile_");
                    // Then we need to concatenate all the parameters
                    parameters = parameters.Concat(profile.GetKeyValuePairs()).ToDictionary(p => p.Key, p => p.Value);
                    // Filter out the Profile. and Global. from Uri Parameters, since they don't have values anyway
                    parameters = parameters.Concat(question.Uris[i].UriParameters.Where(p => !p.Key.StartsWith("Profile.") && !p.Key.StartsWith("Global.")).ToDictionary(p => p.Key, p => p.Value)).ToDictionary(p => p.Key, p => p.Value);
                    formattedUri = SmartFormat.Smart.Format(formattedUri, parameters);

                    // Get the access token
                    var access_token = "";
                    try
                    {
                        // Do a regular auth if not for Cosmos
                        if (!formattedUri.Contains("documents.azure.com"))
                            access_token = await authProvider.AzureAuthorizeAsync(profile.GetSecretsForAuth());
                        else
                        {
                            // We should have a Resource Group in the uri parameters, else auth won't work
                            var resourceGroup = question.Uris[i].UriParameters.Where(p => p.Key == "ResourceGroupName").Select(p => p.Value).FirstOrDefault();
                            access_token = authProvider.CosmosAuthorizeAsync(profile.GetSecretsForAuth(), formattedUri, resourceGroup).GetAwaiter().GetResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        correctAsnwers.Add(new KeyValuePair<string, bool>("Could not complete authorization step. Please check the values in your profile", false));
                        return correctAsnwers;
                    }

                    var response = "";
                    try
                    {
                        // Clear the additionalHeaders by setting to null
                        additionalHeaders = null;

                        if (formattedUri.Contains("documents.azure.com"))
                        {
                            // We need to add additional headers for CosmosDb calls
                            additionalHeaders = new List<KeyValuePair<string, string>>();
                            additionalHeaders.Add(new KeyValuePair<string, string>("x-ms-date", DateTime.UtcNow.ToString("R")));
                            additionalHeaders.Add(new KeyValuePair<string, string>("x-ms-version", "2018-12-31"));
                            // If we are trying to get a doc, we need to pass the Partion Key in the header
                            if (formattedUri.Contains("/docs/"))
                            {
                                var partKey = question.Uris[i].UriParameters.Where(p => p.Key == "PartitionKey").Select(p => p.Value).FirstOrDefault();
                                additionalHeaders.Add(new KeyValuePair<string, string>("x-ms-documentdb-partitionkey", partKey));
                            }
                        }

                        response = await restProvider.GetAsync(formattedUri, access_token, additionalHeaders);
                    }
                    catch (Exception ex)
                    {
                        correctAsnwers = new List<KeyValuePair<string, bool>>();
                        correctAsnwers.Add(new KeyValuePair<string, bool>("Calling one of the APIs to check your answer failed. Either the resource requested has not been created or is still being created. Try in a while.", false));
                        return correctAsnwers;
                    }

                    JObject o = JObject.Parse(response);

                    // If we don't need to check any answers, the call was successful
                    if (question.Answers[i].AnswerParameters == null || question.Answers[i].AnswerParameters.Count == 0)
                    {
                        correctAsnwers.Add(new KeyValuePair<string, bool>("Call succeeded", true));
                    }
                    else
                    {

                        // Special case for RUs. We need to check the offer, and for that we need the database and collection Ids, to make sure we are checking the right collection for the RUs
                        if (formattedUri.Contains("documents.azure.com") && formattedUri.ToLower().EndsWith("/offers"))
                        {
                            // First get the database and collection id
                            var formattedUriForRUs = SmartFormat.Smart.Format(
                                formattedUri.Substring(0, formattedUri.IndexOf(".documents.azure.com")) + ".documents.azure.com/dbs/{DatabaseName}/colls/{CollectionName}", parameters);
                            // We should have a Resource Group in the uri parameters, else auth won't work
                            var resourceGroup = question.Uris[i].UriParameters.Where(p => p.Key == "ResourceGroupName").Select(p => p.Value).FirstOrDefault();
                            var throughput = question.Answers[i].AnswerParameters.Where(p => p.Key.Contains("offerThroughput")).Select(p => p.Value).FirstOrDefault();

                            if (!string.IsNullOrWhiteSpace(throughput))
                            {
                                access_token = await authProvider.CosmosAuthorizeAsync(profile.GetSecretsForAuth(), formattedUriForRUs, resourceGroup);
                                var responseForDBandCollIds = "";
                                try
                                {// We need to add additional headers for CosmosDb calls
                                    additionalHeaders = new List<KeyValuePair<string, string>>();
                                    additionalHeaders.Add(new KeyValuePair<string, string>("x-ms-date", DateTime.UtcNow.ToString("R")));
                                    additionalHeaders.Add(new KeyValuePair<string, string>("x-ms-version", "2018-12-31"));

                                    responseForDBandCollIds = await restProvider.GetAsync(formattedUriForRUs, access_token, additionalHeaders);
                                }
                                catch (Exception ex)
                                {
                                    correctAsnwers = new List<KeyValuePair<string, bool>>();
                                    correctAsnwers.Add(new KeyValuePair<string, bool>("Calling one of the APIs to check your answer failed. Either the resource requested has not been created or is still being created. Try in a while.", false));
                                    return correctAsnwers;
                                }

                                dynamic json = JObject.Parse(responseForDBandCollIds);
                                var ids = json._self;

                                dynamic jsonOffer = JObject.Parse(response);
                                var answerForThroughput = question.Answers[i].AnswerParameters.Where(p => p.Key.Contains("offerThroughput")).FirstOrDefault();
                                var found = false;
                                foreach (var j in jsonOffer.Offers)
                                {
                                    if (j.content.offerThroughput == throughput && j.resource == ids)
                                    {
                                        correctAsnwers.Add(new KeyValuePair<string, bool>(answerForThroughput.Key, true));
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    correctAsnwers.Add(new KeyValuePair<string, bool>(answerForThroughput.Key, false));
                                }

                                // Remove the thoughput answer from the answer parameters to check any pending
                                question.Answers[i].AnswerParameters.Remove(answerForThroughput);
                            }

                            foreach (var answer in question.Answers[i].AnswerParameters)
                            {
                                var properties = answer.Key.Split('.').ToList();
                                correctAsnwers.Add(new KeyValuePair<string, bool>(answer.Key, CheckAnswer(o, properties, answer.Value, 0, properties.Count)));
                            }
                        }
                        else
                        {
                            foreach (var answer in question.Answers[i].AnswerParameters)
                            {
                                var properties = answer.Key.Split('.').ToList();
                                correctAsnwers.Add(new KeyValuePair<string, bool>(answer.Key, CheckAnswer(o, properties, answer.Value, 0, properties.Count)));
                            }
                        }

                    }
                }
            }

            return correctAsnwers;
        }

        public static bool CheckAnswer(JObject json, List<string> properties, string value, int index, int maxIndex)
        {
            var thisProperty = json.SelectToken(properties[index]);

            if (thisProperty == null)
            {
                return false;
            }
            else if (thisProperty.Type == JTokenType.String || thisProperty.Type == JTokenType.Integer && index + 1 == maxIndex)
            {
                return value == (string)thisProperty;
            }
            else if (thisProperty.Type == JTokenType.Boolean && index + 1 == maxIndex)
            {
                return bool.Parse(value) == (bool)thisProperty;
            }
            else if (thisProperty.Type == JTokenType.Object)
            {
                if (CheckAnswer((JObject)thisProperty, properties, value, index + 1, maxIndex))
                    return true;
            }
            else if (thisProperty.Type == JTokenType.Array)
            {
                foreach (var j in (JArray)thisProperty)
                {
                    if (j.Type == JTokenType.Object)
                    {
                        if (CheckAnswer((JObject)j, properties, value, index + 1, maxIndex))
                            return true;
                    }
                    else if (j.Type == JTokenType.String && index + 1 == maxIndex)
                        return value == (string)j;
                }
            }

            return false;
        }

    }
}
