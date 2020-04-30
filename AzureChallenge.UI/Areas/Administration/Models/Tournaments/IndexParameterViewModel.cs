using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace AzureChallenge.UI.Areas.Administration.Models.Tournaments
{
    public class IndexParameterViewModel
    {
        public string TournamentId { get; set; }
        public List<ParameterItem> ParameterList { get; set; }
    }

    public class ParameterItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
