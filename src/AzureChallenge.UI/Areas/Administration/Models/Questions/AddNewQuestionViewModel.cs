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

        public string QuestionType { get; set; }

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

        public List<string> TextParameters { get; set; }

        [Display(Name = "Uri endpoints to call")]
        public List<UriList> Uris { get; set; }

        public List<string> GlobalParameters { get; set; }
        
        [Display(Name="Justification - Show after successful completion of question")]
        public string Justification { get; set; }

        [Display(Name="Useful Links")]
        public List<string> UsefulLinks { get; set; }

        public string Owner { get; set; }

        public class UriList
        {
            public int Id { get; set; }

            [Required]
            public string Uri { get; set; }

            public string CallType { get; set; }

            public List<string> UriParameters { get; set; }

            public bool RequiresContributorAccess { get; set; }
        }

        public List<string> AvailableParameters { get; set; }
    }
}
