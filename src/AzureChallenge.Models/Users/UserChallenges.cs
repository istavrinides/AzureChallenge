using AzureChallenge.Models.Questions;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureChallenge.Models.Users
{
    public class UserChallenges : AzureChallengeDocument
    {
        public UserChallenges() : base("UserChallenges") { }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "challenges")]
        public List<UserChallengeItem> Challenges { get; set; }
    }

    public class UserChallengeItem
    {
        [JsonProperty(PropertyName = "challengeId")]
        public string ChallengeId { get; set; }
        [JsonProperty(PropertyName = "currentQuestion")]
        public string CurrentQuestion { get; set; }
        [JsonProperty(PropertyName = "currentIndex")]
        public int CurrentIndex { get; set; }
        [JsonProperty(PropertyName = "numOfQuestions")]
        public int NumOfQuestions { get; set; }
        [JsonProperty(PropertyName = "accumulatedXP")]
        public int AccumulatedXP { get; set; }
        [JsonProperty(PropertyName = "startTimeUTC")]
        public DateTime StartTimeUTC { get; set; }
        [JsonProperty(PropertyName = "endTimeUTC")]
        public DateTime endTimeUTC { get; set; }
        [JsonProperty(PropertyName = "completed")]
        public bool Completed { get; set; }
        [JsonProperty(PropertyName = "numOfEfforts")]
        public int NumOfEfforts { get; set; }
    }
}
