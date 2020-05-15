using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Interfaces.Providers.REST
{
    public interface IAzureAuthProvider
    {
        public Task<string> AzureAuthorizeAsync(IEnumerable<KeyValuePair<string, string>> secrets);
        public Task<string> CosmosAuthorizeAsync(IEnumerable<KeyValuePair<string, string>> secrets, string uri, string resourceGroup = "");
    }
}
