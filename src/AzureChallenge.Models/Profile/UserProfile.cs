using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AzureChallenge.Models.Profile
{
    public class UserProfile
    {
        public string SubscriptionId { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string TenantId { get; set; }

        public string Email { get; set; }

        public Dictionary<string, string> GetKeyValuePairs()
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>();

            pairs.Add("Profile_SubscriptionId", SubscriptionId);
            pairs.Add("Profile_ClientId", ClientId);
            pairs.Add("Profile_ClientSecret", ClientSecret);
            pairs.Add("Profile_TenantId", TenantId);
            pairs.Add("Profile_Email", Email);

            using (var md5Hasher = MD5.Create())
            {
                var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(Email));
                pairs.Add("Profile_UserNameHashed", "a" + BitConverter.ToString(data).Replace("-", "").Substring(0, 16).ToLower());
            }

            return pairs;
        }

        public IEnumerable<KeyValuePair<string, string>> GetSecretsForAuth(string uri = "")
        {
            var secrets = new List<KeyValuePair<string, string>>();

            secrets.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            secrets.Add(new KeyValuePair<string, string>("client_id", ClientId));
            secrets.Add(new KeyValuePair<string, string>("client_secret", ClientSecret));

            if (uri.Contains("vault.azure.net"))
            {
                secrets.Add(new KeyValuePair<string, string>("scope", "https://vault.azure.net/.default"));
            }
            else
            {
                secrets.Add(new KeyValuePair<string, string>("resource", "https://management.azure.com"));
            }
            secrets.Add(new KeyValuePair<string, string>("TenantId", TenantId));
            secrets.Add(new KeyValuePair<string, string>("SubscriptionId", SubscriptionId));

            return secrets;
        }
    }
}
