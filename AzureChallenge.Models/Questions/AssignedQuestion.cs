using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureChallenge.Models.Questions
{
    public class AssignedQuestion : AzureChallengeDocument
    {
        public AssignedQuestion() : base("AssignedQuestion") { }

        [JsonProperty(PropertyName = "id")]
        public string QuestionId { get; set; }

        [JsonProperty(PropertyName = "associatedQuestionId")]
        public string AssociatedQuestionId { get; set; }

        [JsonProperty(PropertyName = "challengeId")]
        public string ChallengeId { get; set; }

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
        public Dictionary<string, string> TextParameters { get; set; }

        [JsonProperty(PropertyName = "justification")]
        public string Justification { get; set; }

        [JsonProperty(PropertyName = "usefulLinks")]
        public List<string> UsefulLinks { get; set; }

        [JsonProperty(PropertyName = "urilist")]
        public List<UriList> Uris { get; set; }

        [JsonProperty(PropertyName = "answerlist")]
        public List<AnswerList> Answers { get; set; }

        public class UriList
        {
            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "uri")]
            public string Uri { get; set; }

            [JsonProperty(PropertyName = "calltype")]
            public string CallType { get; set; }

            [JsonProperty(PropertyName = "uriParameters")]
            public Dictionary<string, string> UriParameters { get; set; }
        }

        public class AnswerList
        {
            [JsonProperty(PropertyName = "associatedquestionid")]
            public int AssociatedQuestionId { get; set; }

            [JsonProperty(PropertyName = "answers")]
            public List<AnswerParameterItem> AnswerParameters { get; set; }

            [JsonProperty(PropertyName = "responsetype")]
            public string ResponseType { get; set; }
        }

        public class AnswerParameterItem
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
