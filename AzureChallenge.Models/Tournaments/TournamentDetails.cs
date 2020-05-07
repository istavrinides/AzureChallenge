using AzureChallenge.Models.Questions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureChallenge.Models.Tournaments
{
    public class TournamentDetails : AzureChallengeDocument
    {
        public TournamentDetails() : base("Tournament") { }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "descritpion")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "questions")]
        public List<QuestionLite> Questions { get; set; }
    }
}
