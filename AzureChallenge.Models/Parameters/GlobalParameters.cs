using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace AzureChallenge.Models.Parameters
{
    public class GlobalParameters : AzureChallengeDocument
    {
        public GlobalParameters() : base("GlobalParameters") { }
        
        [JsonProperty(PropertyName = "id")]
        public string TournamentId { get; set; }
        [JsonProperty(PropertyName = "parameters")]
        public List<ParameterDefinition> Parameters { get; set; }

        public class ParameterDefinition
        {
            [JsonProperty(PropertyName = "key")]
            public string Key { get; set; }
            [JsonProperty(PropertyName = "value")]
            public string Value { get; set; }
            [JsonProperty(PropertyName = "assignedToQuestion")]
            public int AssignedToQuestion { get; set; }
        }
    }
}
