using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AzureChallenge.UI.Areas.Administration.Models.Challenges
{
    public class IndexChallengeViewModel
    {

        public IList<ChallengeList> Challenges { get; set; }

        public List<String> AzureServiceCategories { get; set; }
        [Display(Name = "Azure Service Category")]
        public string AzureServiceCategory { get; set; }
        [Display(Name = "Available files")]
        public List<KeyValuePair<string, string>> FilesAvailableForImport { get; set; }
        
        [Display(Name = "Welcome message", Prompt = "What they user sees when they start the Challenge")]
        [Required]
        public string WelcomeMessage { get; set; }
        public List<string> PrereqLinks { get; set; }
        
        public int Duration { get; set; }

        /// <summary>
        /// If false, users have unlimited tries on questions
        /// If true, each incorrect answer will deduct 25% (so 4 tries to get it right). After that, user will get 0 points
        /// </summary>
        [Display(Name = "Track and deduct points?")]
        public bool TrackAndDeductPoints { get; set; }
    }

    public class ChallengeList
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsLocked { get; set; }
        public string AzureServiceCategory { get; set; }
    }

    public class IndexChallengeViewModelFromPost
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsLocked { get; set; }
        public string AzureServiceCategory { get; set; }
        public string WelcomeMessage { get; set; }
        public List<string> PrereqLinks { get; set; }
        public int Duration { get; set; }

        /// <summary>
        /// If false, users have unlimited tries on questions
        /// If true, each incorrect answer will deduct 25% (so 4 tries to get it right). After that, user will get 0 points
        /// </summary>
        [Display(Name = "Track and deduct points?")]
        public bool TrackAndDeductPoints { get; set; }
    }

    public class GitHubFiles
    {
        public string name { get; set; }
        public string download_url { get; set; }
    }
}
