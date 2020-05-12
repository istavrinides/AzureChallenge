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
        private readonly IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalParameters> parametersProvider;

        public AssignedQuestionProvider(IDataProvider<AzureChallengeResult, AssignedQuestion> dataProvider,
                                        IAzureAuthProvider authProvider,
                                        IRESTProvider restProvider,
                                        IParameterProvider<ACM.AzureChallengeResult, ACMP.GlobalParameters> parametersProvider)
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

            if (!result.Item1.Success)
                return null;

            var question = result.Item2;

            // Get the access token
            var access_token = await authProvider.AuthorizeAsync(profile.GetSecretsForAuth());

            // Create a list to check validity of answers
            List<KeyValuePair<string, bool>> correctAsnwers = new List<KeyValuePair<string, bool>>();

            // Get the global parameters
            var globalParameters = await parametersProvider.GetItemAsync(question.TournamentId);
            // Global parameters don't have the Global. prefix, so create a new dictionary with that as the key name
            var parameters = new Dictionary<string, string>();
            foreach (var gp in globalParameters.Item2.Parameters)
            {
                parameters.Add($"Global_{gp.Key}", gp.Value);
            }

            for (int i = 0; i < question.Uris.Count; i++)
            {
                if (question.Uris[0].CallType == "GET")
                {
                    // Prepare the uri
                    // First we need to replace the . notation for Global and Profile placeholders to _ (SmartFormat doesn't like the . notation)
                    var formattedUri = question.Uris[0].Uri.Replace("Global.", "Global_").Replace("Profile.", "Profile_");
                    // Then we need to concatenate all the parameters
                    parameters = parameters.Concat(profile.GetKeyValuePairs()).ToDictionary(p => p.Key, p => p.Value);
                    // Filter out the Profile. and Global. from Uri Parameters, since they don't have values anyway
                    parameters = parameters.Concat(question.Uris[0].UriParameters.Where(p => !p.Key.StartsWith("Profile.") && !p.Key.StartsWith("Global.")).ToDictionary(p => p.Key, p => p.Value)).ToDictionary(p => p.Key, p => p.Value);
                    formattedUri = SmartFormat.Smart.Format(formattedUri, parameters);

                    var response = await restProvider.GetAsync(formattedUri, access_token);

                    JObject o = JObject.Parse(response);

                    foreach (var answer in question.Answers[i].AnswerParameters)
                    {
                        var properties = answer.Key.Split('.').ToList();
                        correctAsnwers.Add(new KeyValuePair<string, bool>(answer.Key, CheckAnswer(o, properties, answer.Value, 0, properties.Count)));
                    }

                }
                else if (question.Uris[0].CallType == "HEAD")
                {
                    var response = await restProvider.HeadAsync(SmartFormat.Smart.Format(question.Uris[0].Uri, question.Uris[0].UriParameters), access_token);
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
            else if (thisProperty.Type == JTokenType.String && index + 1 == maxIndex)
            {
                return value == (string)thisProperty;
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
