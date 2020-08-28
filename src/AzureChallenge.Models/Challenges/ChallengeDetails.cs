using AzureChallenge.Models.Questions;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureChallenge.Models.Challenges
{
    public class ChallengeDetails : AzureChallengeDocument
    {
        public ChallengeDetails() : base("Challenge") { }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "isPublic")]
        public bool IsPublic { get; set; }

        [JsonProperty(PropertyName = "isLocked")]
        public bool IsLocked { get; set; }

        [JsonProperty(PropertyName = "questions")]
        public List<QuestionLite> Questions { get; set; }

        [JsonProperty(PropertyName = "azureCategory")]
        public string AzureServiceCategory { get; set; }

        [JsonProperty(PropertyName = "welcomeMessage")]
        public string WelcomeMessage { get; set; }

        [JsonProperty(PropertyName = "prereqLinks")]
        public List<string> PrereqLinks { get; set; }

        [JsonProperty(PropertyName = "duration")]
        public int Duration { get; set; }
    }
}
