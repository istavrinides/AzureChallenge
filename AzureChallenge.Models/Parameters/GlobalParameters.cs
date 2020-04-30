using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureChallenge.Models.Parameters
{
    public class GlobalParameters : AzureChallengeDocument
    {
        public GlobalParameters() : base("GlobalParameters") { }
        
        [JsonProperty(PropertyName = "id")]
        public string TournamentId { get; set; }
        [JsonProperty(PropertyName = "parameters")]
        public Dictionary<string, string> Parameters { get; set; }
    }
}
