using AzureChallenge.Models.Questions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureChallenge.Models.Tournaments
{
    public class TournamentDetails : AzureChallengeDocument
    {
        public TournamentDetails() : base("Tournament") { }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<QuestionLite> Questions { get; set; }
    }
}
