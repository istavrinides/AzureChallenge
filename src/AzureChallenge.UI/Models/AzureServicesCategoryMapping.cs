using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureChallenge.UI.Models
{
    public static class AzureServicesCategoryMapping
    {
        public static List<string> CategoryName
        {
            get
            {
                return new List<string>
                {
                    "AI + Machine Learning",
                    "Analytics",
                    "Blockchain",
                    "Compute", 
                    "Containers",
                    "Databases",
                    "Developer Tools",
                    "DevOps",
                    "Hybrid",
                    "Identity",
                    "Integration",
                    "Internet of Things",
                    "Management and Governance",
                    "Media",
                    "Migration",
                    "Mixed Reality",
                    "Mobile",
                    "Networking",
                    "Security",
                    "Storage",
                    "Web",
                    "Windows Virtual Desktop"
                };
            }
        }

    }
}
