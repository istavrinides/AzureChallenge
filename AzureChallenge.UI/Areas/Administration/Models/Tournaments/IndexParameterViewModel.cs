using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace AzureChallenge.UI.Areas.Administration.Models.Challenges
{
    public class IndexParameterViewModel
    {
        public string ChallengeId { get; set; }
        public List<ParameterItem> ParameterList { get; set; }

        public class ParameterItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public int AssignedToQuestion { get; set; }
        }
    }
}
