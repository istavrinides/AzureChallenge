using AzureChallenge.Interfaces.Providers.REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Providers.RESTProviders
{
    public class RESTProvider : IRESTProvider
    {
        public async Task<string> GetAsync(string uri, string authorizationHeader)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorizationHeader);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri)
                };

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }
        public async Task<Dictionary<string, string>> HeadAsync(string uri, string authorizationHeader)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorizationHeader);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Head,
                    RequestUri = new Uri(uri)
                };

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return response.Headers.ToDictionary(p => p.Key, p => string.Join(";", p.Value));
            }
        }

        public async Task<string> PostAsync(string uri, IEnumerable<KeyValuePair<string, string>> body, IRESTProvider.ContentType contentType = IRESTProvider.ContentType.Json)
        {
            using (var httpClient = new HttpClient())
            {
                if (contentType == IRESTProvider.ContentType.FormUrlEncoded)
                {   
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(uri),
                        Content = new FormUrlEncodedContent(body)
                    };

                    var response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
                else if (contentType == IRESTProvider.ContentType.Json)
                {

                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(uri)
                    };

                    var response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }

                return "";
            }
        }
    }
}
