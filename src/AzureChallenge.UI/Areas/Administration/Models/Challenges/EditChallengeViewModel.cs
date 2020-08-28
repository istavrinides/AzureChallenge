using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AzureChallenge.UI.Areas.Administration.Models.Challenges
{
    public class EditChallengeViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Challenge Name")]
        public string Name { get; set; }

        public string Description { get; set; }

        public string PrivateJoinCode { get; set; }

        [Display(Name="Will the challenge be publically visible (users can search for and join)?")]
        public bool IsPublic { get; set; }

        public bool IsLocked { get; set; }

        public bool OldIsPublic { get; set; }

        public List<ChallengeQuestion> ChallengeQuestions { get; set; }

        [Display(Name = "Available Questions")]
        public List<Question> Questions { get; set; }

        public AssignedQuestion QuestionToAdd { get; set; }

        public UserProfile CurrentUserProfile { get; set; }
        public string AzureServiceCategory { get; set; }

        public class UserProfile
        {
            public string SubscriptionId { get; set; }
            public string UserNameHashed { get; set; }
            public string TenantId { get; set; }
        }
        public string WelcomeMessage { get; set; }
        public List<string> PrereqLinks { get; set; }

        public int Duration { get; set; }
    }

    public class ChallengeQuestion
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
        public string QuestionType { get; set; }
        public string AssociatedQuestionId { get; set; }
        public string ChallengeId { get; set; }
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
            public bool RequiresContributorAccess { get; set; }
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
