using AzureChallenge.Interfaces.Providers.REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallenge.Providers.RESTProviders
{
    public class RESTProvider : IRESTProvider
    {
        public async Task<(string Content, HttpStatusCode StatusCode)> GetAsync(string uri, string authorizationHeader, List<KeyValuePair<string, string>> additionalHeaders)
        {
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = null;

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorizationHeader);

                // All Cosmos Db calls expect the current UTC time in RFC1123 format and the API version in the header variables
                if (uri.Contains("documents.azure.com"))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationHeader);
                }

                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri)
                };

                if (additionalHeaders != null)
                {
                    foreach (var h in additionalHeaders)
                    {
                        // If the key is x-ms-documentdb-partitionkey, the value needs to be in an array
                        if (h.Key == "x-ms-documentdb-partitionkey")
                        {
                            request.Headers.Add(h.Key, Newtonsoft.Json.JsonConvert.SerializeObject(new[] { h.Value }));
                        }
                        else
                            request.Headers.Add(h.Key, h.Value);
                    }
                }

                var content = "";

                try
                {
                    response = await httpClient.SendAsync(request);
                    content = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                }
                catch(Exception ex)
                {
                    content += "\n\n" + ex.Message;
                }

                return (Content: content, StatusCode: response != null ? response.StatusCode : HttpStatusCode.InternalServerError);
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
