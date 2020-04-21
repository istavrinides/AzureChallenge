using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security;

namespace AzureChallenge.Models.Questions
{
    public class Question : AzureChallengeDocument
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "textParameters")]
        public Dictionary<string, string>   TextParameters { get; set; }

        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

        [JsonProperty(PropertyName = "uriParameters")]
        public Dictionary<string, string> UriParameters { get; set; }

        [JsonProperty(PropertyName = "answers")]
        public Dictionary<string, string> Answers { get; set; }

        [JsonProperty(PropertyName = "calltype")]
        public string CallType { get; set; }

        [JsonProperty(PropertyName = "responsetype")]
        public string ResponseType { get; set; }

        public Question() : base("Question") { }
    }
}
