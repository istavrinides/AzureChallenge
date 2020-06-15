using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    }
}
