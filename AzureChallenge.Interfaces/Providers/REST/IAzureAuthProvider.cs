using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Interfaces.Providers.REST
{
    public interface IAzureAuthProvider
    {
        public Task<string> AuthorizeAsync(IEnumerable<KeyValuePair<string, string>> secrets);
    }
}
