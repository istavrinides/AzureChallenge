using AzureChallenge.Interfaces.Providers.REST;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Providers.RESTProviders
{
    public class AzureAuthProvider : IAzureAuthProvider
    {
        private readonly IRESTProvider restProvider;

        public AzureAuthProvider(IRESTProvider restProvider)
        {
            this.restProvider = restProvider;
        }

        /// <summary>
        /// Authorizes the current session and get an access token back
        /// </summary>
        /// <param name="secrets">A list of KeyValue pais that contain the ClientId, ClientSecret, SubscriptionId and TenantId</param>
        /// <returns>The access token</returns>
        public async Task<string> AzureAuthorizeAsync(IEnumerable<KeyValuePair<string, string>> secrets)
        {
            // Azure API Authorization flow
            var tenantId = secrets.Where(p => p.Key == "TenantId").Select(p => p.Value).FirstOrDefault();
            var authUri = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";

            var response = await restProvider.PostAsync(authUri, secrets, IRESTProvider.ContentType.FormUrlEncoded);

            dynamic json = JObject.Parse(response);

            return json.access_token;
        }

        public async Task<string> CosmosAuthorizeAsync(IEnumerable<KeyValuePair<string, string>> secrets, string uri, string resourceGroup = "")
        {
            // We need to get the account name
            var accountName = uri.StartsWith("https://") ? uri.Substring(8).Split('.').First() : uri.Substring(7).Split('.').First();

            string azureAccessToken = await AzureAuthorizeAsync(secrets);

            // Now get the Cosmos Db key
            var subscriptionId = secrets.Where(p => p.Key == "SubscriptionId").Select(p => p.Value).FirstOrDefault();
            var response = await restProvider.GetAsync($"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.DocumentDB/databaseAccounts/{accountName}/readonlykeys?api-version=2019-12-12", azureAccessToken, null);

            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return "";
            }

            dynamic json = JObject.Parse(response.Content);
            string cosmosDbAccessKey = json.primaryReadonlyMasterKey;

            // Parse the uri on /

            // We need the resource type and link
            string resourceType = "", resourceLink = uri.Substring(uri.IndexOf("dbs") > 0 ? uri.IndexOf("dbs") : 0);
            if (uri.Contains("offers"))
            {
                resourceType = "offers";
                // Special case, only for offers
                resourceLink = uri.Split('/').Last() == "offers" ? "" : uri.Split('/').Last().ToLower();
            }
            else if (uri.Contains("permissions")) resourceType = "permissions";
            else if (uri.Contains("users")) resourceType = "users";
            else if (uri.Contains("triggers")) resourceType = "triggers";
            else if (uri.Contains("udfs")) resourceType = "udfs";
            else if (uri.Contains("sprocs")) resourceType = "sprocs";
            else if (uri.Contains("attachments")) resourceType = "attachments";
            else if (uri.Contains("docs")) resourceType = "docs";
            else if (uri.Contains("colls")) resourceType = "colls";
            else resourceType = "dbs";

            // We also need the UTC data in a specific format
            var date = DateTime.UtcNow.ToString("R");

            // Construct the string to sign
            string payLoad = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}\n{1}\n{2}\n{3}\n{4}\n",
                "get",
                resourceType.ToLowerInvariant(),
                resourceLink,
                date.ToLowerInvariant(),
                ""
            );

            var hmacSha256 = new System.Security.Cryptography.HMACSHA256 { Key = Convert.FromBase64String(cosmosDbAccessKey) };

            byte[] hashPayLoad = hmacSha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payLoad));
            string signature = Convert.ToBase64String(hashPayLoad);

            return System.Web.HttpUtility.UrlEncode(
                String.Format(System.Globalization.CultureInfo.InvariantCulture, "type={0}&ver={1}&sig={2}",
                "master",
                "1.0",
                signature));
        }
    }
}
