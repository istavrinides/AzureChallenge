using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace AzureChallenge.Models.Parameters
{
    public class GlobalChallengeParameters : AzureChallengeDocument
    {
        public GlobalChallengeParameters() : base("GlobalChallengeParameters") { }
        
        [JsonProperty(PropertyName = "id")]
        public string ChallengeId { get; set; }
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
