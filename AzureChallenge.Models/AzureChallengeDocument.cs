using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureChallenge.Models
{
    public class AzureChallengeDocument
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        public AzureChallengeDocument(string type)
        {
            Type = type;
        }
    }
}
