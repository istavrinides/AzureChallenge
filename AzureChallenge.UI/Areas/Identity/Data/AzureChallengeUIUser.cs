using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AzureChallenge.UI.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the AzureChallengeUIUser class
    public class AzureChallengeUIUser : IdentityUser
    {
        [ProtectedPersonalData]
        public string SubscriptionId { get; set; }

        [ProtectedPersonalData]
        public string ClientId { get; set; }

        [ProtectedPersonalData]
        public string ClientSecret { get; set; }

        [ProtectedPersonalData]
        public string TenantId { get; set; }

        [ProtectedPersonalData]
        public string SubscriptionName { get; set; }
    }
}
