using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AzureChallenge.UI.Areas.Administration.Models.Tournaments
{
    public class EditTournamentViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Tournament Name")]
        public string Name { get; set; }

        public string Description { get; set; }
        public List<TournamentQuestion> TournamentQuestions { get; set; }

        [Display(Name = "Available Questions")]
        public List<Question> Questions { get; set; }

        public AssignedQuestion QuestionToAdd { get; set; }

        public UserProfile CurrentUserProfile { get; set; }

        public class UserProfile
        {
            public string SubscriptionId { get; set; }
            public string UserNameHashed { get; set; }
            public string TenantId { get; set; }
        }
    }

    public class TournamentQuestion
    {
        public string Id { get; set; }
        public string AssociatedQuestionId { get; set; }
        public string Name { get; set; }
        public int Difficulty { get; set; }
        public string Description { get; set; }
        public int Index { get; set; }
        public string NextQuestionId { get; set; }

        public string DifficultyString => this.Difficulty == 1 ? "Easy" : this.Difficulty == 2 ? "Medium" : this.Difficulty == 3 ? "Hard" : "Expert";
    }

    public class AssignedQuestion
    {
        public string Id { get; set; }
        public string AssociatedQuestionId { get; set; }
        public string TournamentId { get; set; }
        public string Name { get; set; }
        public string TargettedAzureService { get; set; }
        public int Difficulty { get; set; }
        public string Description { get; set; }
        public string Text { get; set; }
        public List<KVPair> TextParameters { get; set; }
        public string Justification { get; set; }
        public List<string> UsefulLinks { get; set; }

        public List<UriList> Uris { get; set; }

        public List<AnswerList> Answers { get; set; }

        public class UriList
        {
            public int Id { get; set; }
            public string Uri { get; set; }
            public string CallType { get; set; }
            public List<KVPair> UriParameters { get; set; }
        }

        public class AnswerList
        {
            public int AssociatedQuestionId { get; set; }

            public List<KVPair> AnswerParameters { get; set; }
            public string ResponseType { get; set; }
        }

        public class KVPair
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public string ErrorMessage { get; set; }
        }
        public string DifficultyString => this.Difficulty == 1 ? "Easy" : this.Difficulty == 2 ? "Medium" : this.Difficulty == 3 ? "Hard" : "Expert";
    }

    public class Question
    {
        public string Id { get; set; }
        public string AzureService { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }
}
