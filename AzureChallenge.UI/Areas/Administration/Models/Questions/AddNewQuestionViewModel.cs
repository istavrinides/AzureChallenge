using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AzureChallenge.UI.Areas.Administration.Models.Questions
{
    public class AddNewQuestionViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Targetted Azure Service")]
        public string TargettedAzureService { get; set; }

        public int Difficulty { get; set; }

        public List<string> AzureServicesList { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Question")]
        public string Text { get; set; }

        public Dictionary<string, string> TextParameters { get; set; }

        [Display(Name = "Uri endpoints to call")]
        public List<UriList> Uris { get; set; }

        public List<AnswerList> Answers { get; set; }

        public class UriList
        {
            public int Id { get; set; }

            public string Uri { get; set; }

            public string CallType { get; set; }

            public Dictionary<string, string> UriParameters { get; set; }
        }

        public class AnswerList
        {
            public int AssociatedQuestionId { get; set; }

            public Dictionary<string, string> AnswerParameters { get; set; }

            public string ResponseType { get; set; }
        }
    }
}
