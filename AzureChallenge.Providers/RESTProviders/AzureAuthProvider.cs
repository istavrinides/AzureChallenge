using AzureChallenge.Interfaces.Providers.REST;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<string> AuthorizeAsync(IEnumerable<KeyValuePair<string, string>> secrets)
        {
            var tenantId = secrets.Where(p => p.Key == "TenantId").Select(p => p.Value).FirstOrDefault();
            var uri = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";

            var response = await restProvider.PostAsync(uri, secrets, IRESTProvider.ContentType.FormUrlEncoded);

            dynamic json = JObject.Parse(response);

            return json.access_token;
        }
    }
}
