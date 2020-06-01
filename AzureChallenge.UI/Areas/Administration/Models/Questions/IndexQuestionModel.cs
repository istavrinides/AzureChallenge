using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureChallenge.UI.Areas.Administration.Models.Questions
{
    public class IndexQuestionViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool CanDelete { get; set; }
        public string TargettedAzureService { get; set; }
    }
}
