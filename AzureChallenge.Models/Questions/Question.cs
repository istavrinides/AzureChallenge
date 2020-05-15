using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security;

namespace AzureChallenge.Models.Questions
{
    public class Question : AzureChallengeDocument
    {
        public Question() : base("Question") { }



        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        
        [JsonProperty(PropertyName = "targettedazureservice")]
        public string TargettedAzureService { get; set; }

        [JsonProperty(PropertyName = "difficulty")]
        public int Difficulty { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "textParameters")]
        public List<string>   TextParameters { get; set; }
        
        [JsonProperty(PropertyName = "urilist")]
        public List<UriList> Uris { get; set; }

        [JsonProperty(PropertyName = "justification")]
        public string Justification { get; set; }

        [JsonProperty(PropertyName = "usefulLinks")]
        public List<string> UsefulLinks { get; set; }

        public class UriList
        {
            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "uri")]
            public string Uri { get; set; }

            [JsonProperty(PropertyName = "calltype")]
            public string CallType { get; set; }

            [JsonProperty(PropertyName = "uriParameters")]
            public List<string> UriParameters { get; set; }
        }

        public string DifficultyString => this.Difficulty == 1 ? "Easy" : this.Difficulty == 2 ? "Medium" : this.Difficulty == 3 ? "Hard" : "Expert";
    }
}
