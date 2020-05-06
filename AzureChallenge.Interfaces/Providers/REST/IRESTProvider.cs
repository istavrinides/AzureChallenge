using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Interfaces.Providers.REST
{
    public interface IRESTProvider
    {
        enum ContentType
        {
            Json,
            FormUrlEncoded
        }

        public Task<string> GetAsync(string uri, string authorizationHeader);

        public Task<Dictionary<string, string>> HeadAsync(string uri, string authorizationHeader);

        public Task<string> PostAsync(string uri, IEnumerable<KeyValuePair<string, string>> body, ContentType contentType = ContentType.Json);
    }
}
