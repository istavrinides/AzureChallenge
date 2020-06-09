using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureChallenge.UI.Models.ChallengeViewModels
{
    public class IndexViewModel
    {
        public List<Challenge> Challenges { get; set; }
        public List<string> AzureServicesCategories { get; set; }
    }

    public class Challenge
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CurrentQuestionId { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsComplete { get; set; }
        public bool IsUnderway { get; set; }
        public string AzureCategory { get; set; }
    }

    public class QuestionViewModel
    {
        public string QuestionId { get; set; }
        public int QuestionIndex { get; set; }
        public bool ThisQuestionDone { get; set; }
        public string ChallengeId { get; set; }
        public string PreviousQuestionId { get; set; }
        public string NextQuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionName { get; set; }
        public string TournamentName { get; set; }
        public int Difficulty { get; set; }
        public string Justification { get; set; }
        public List<string> HelpfulLinks { get; set; }
        public bool ShowWarning { get; set; }
        public string WarningMessage { get; set; }
    }

    public class JoinPrivateChallengeViewModel
    {
        public string ChallengeId { get; set; }
    }
}
