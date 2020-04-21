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
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public Dictionary<string, string> TextParameters { get; set; }

        [Required]
        public string Uri { get; set; }

        [Required]
        public Dictionary<string, string> UriParameters { get; set; }

        [Required]
        public Dictionary<string, string> Answers { get; set; }

        [Required]
        public string CallType { get; set; }

        [Required]
        public string ResponseType { get; set; }
    }
}
