using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AzureChallenge.UI.Areas.Administration.Models.Challenges
{
    public class AnalyticsChallengeViewModel
    {
        public int Started { get; set; }
        public int Finished { get; set; }
        public string ChallengeId { get; set; }

        // Challenge progress is a list of strings
        // Each string has the following format <userId>:<questionIndex>:<year>:<month>:<day>
        // Year/Month/Day is used to filter out older question completions, as we are only considering event from the last 5 days
        public List<string> ChallengeProgress { get; set; }
    }
}
