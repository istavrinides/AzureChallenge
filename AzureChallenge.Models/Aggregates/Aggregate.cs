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

        [JsonProperty(PropertyName = "challenge")]
        public ChallengeAggregate Challenge { get; set; }
    }

    public class ChallengeAggregate
    {
        [JsonProperty(PropertyName = "totalPublic")]
        public int TotalPublic { get; set; }
    }
}
