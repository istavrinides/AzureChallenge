using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureChallenge.Interfaces.Providers.Aggregates;
using AzureChallenge.Models.Questions;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using ACM = AzureChallenge.Models;
using ACMA = AzureChallenge.Models.Aggregates;

namespace AzureChallenge.UI.Hubs
{
    public class ChallengeHub : Hub
    {
        private readonly IAggregateProvider<ACM.AzureChallengeResult, ACMA.Aggregate> aggregateProvider;

        public ChallengeHub(IAggregateProvider<ACM.AzureChallengeResult, ACMA.Aggregate> aggregateProvider)
        {
            this.aggregateProvider = aggregateProvider;
        }

        public async Task SendQuestionCompletionToGroup(string userId, string challengeId, string questionIndex)
        {
            string template = $"{userId}:{questionIndex}:{DateTime.Now.Year}:{DateTime.Now.Month}:{DateTime.Now.Day}";
            await Clients.Group(challengeId).SendAsync("QuestionComplete", template);

            var aggregatesReponse = await aggregateProvider.GetItemAsync(challengeId);

            if (aggregatesReponse.Item1.Success && aggregatesReponse.Item2 != null)
            {
                var agg = aggregatesReponse.Item2;

                if (agg.ChallengeUsers != null)
                {
                    List<string> newProgress = new List<string>();

                    if (agg.ChallengeUsers.ChallengeProgress.Count > 0)
                    {
                        var added = false;

                        foreach (var item in agg.ChallengeUsers.ChallengeProgress)
                        {
                            var parsed = item.Split(':');
                            var itemUserId = parsed[0];
                            var itemQuestionIndex = parsed[1];

                            // If it's not the current user
                            if (itemUserId != userId)
                                newProgress.Add(item);
                            // If it is the current user, check if the new question index is higher (avoids adding double entries for answering previous questions again)
                            else if (int.Parse(questionIndex) > int.Parse(itemQuestionIndex))
                            {
                                newProgress.Add(template);
                                added = true;
                            }
                        }

                        // Didn't find it
                        if (!added)
                        {
                            newProgress.Add(template);
                        }
                    }
                    else
                    {
                        newProgress.Add(template);
                    }

                    agg.ChallengeUsers.ChallengeProgress = newProgress;
                    await aggregateProvider.AddItemAsync(agg);

                }
            }
        }

        public async Task SendChallengeStartToGroup(string userId, string challengeId, string questionIndex)
        {
            await Clients.Group(challengeId).SendAsync("ChallengeStart", $"{userId}:{questionIndex}");
        }

        public async Task SendChallengeCompleteToGroup(string userId, string challengeId, string questionIndex)
        {
            await Clients.Group(challengeId).SendAsync("ChallengeComplete", $"{userId}:{questionIndex}");
        }

        public async Task AddToGroup(string challengeId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, challengeId);

            var aggregatesReponse = await aggregateProvider.GetItemAsync(challengeId);

            if (aggregatesReponse.Item1.Success && aggregatesReponse.Item2 != null)
            {
                var agg = aggregatesReponse.Item2;

                if (agg.ChallengeUsers != null)
                {
                    await Clients.Group(challengeId).SendAsync("ChallengeHistory", JsonConvert.SerializeObject(agg.ChallengeUsers.ChallengeProgress));
                }
            }
        }

        public async Task RemoveFromGroup(string challengeId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, challengeId);
        }
    }
}
