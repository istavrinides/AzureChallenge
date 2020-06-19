using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace AzureChallenge.Models.Aggregates
{
    public class Aggregate : AzureChallengeDocument
    {
        public Aggregate() : base("Aggregate") { }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "challengeTotals")]
        public ChallengeAggregateTotals ChallengeTotals { get; set; }

        [JsonProperty(PropertyName = "challengeUsers")]
        public ChallengeAggregateUsers ChallengeUsers { get; set; }
    }

    public class ChallengeAggregateTotals
    {
        [JsonProperty(PropertyName = "totalPublic")]
        public int TotalPublic { get; set; }
    }

    public class ChallengeAggregateUsers
    {
        [JsonProperty(PropertyName = "started")]
        public int Started { get; set; }
        [JsonProperty(PropertyName = "finished")]
        public int Finished { get; set; }

        // Challenge progress is a list of strings
        // Each string has the following format <userId>:<questionIndex>:<year>:<month>:<day>
        // Year/Month/Day is used to filter out older question completions, as we are only considering event from the last 5 days
        [JsonProperty(PropertyName = "challengeProgress")]
        public List<string> ChallengeProgress { get; set; }
    }
}
