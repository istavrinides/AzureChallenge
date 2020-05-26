using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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

        [ProtectedPersonalData]
        public int AccumulatedPoint { get; set; }

        public string UserNameHashed()
        {
            using (var md5Hasher = MD5.Create())
            {
                var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(this.UserName));
                return $"a{BitConverter.ToString(data).Replace("-", "").Substring(0, 16).ToLower()}";
            }
        }

        public string UserNameSanitized()
        {
            return System.Text.RegularExpressions.Regex.Replace(this.UserName, "[^a-zA-Z0-9]+", "", System.Text.RegularExpressions.RegexOptions.Compiled);
        }
    }
}
