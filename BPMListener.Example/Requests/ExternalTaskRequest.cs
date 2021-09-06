using BPMListener.Example.Exceptions;
using BPMListener.Example.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BPMListener.Example.Requests
{
    public class ExternalTaskRequest : Request
    {
        public async Task<IEnumerable<ExternalTask>> GetAsync()
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "external-task");
            using var http = GetHttpClient();
            var response = await http.SendAsync(httpRequest);
            var s = await response.Content.ReadAsStringAsync();
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new ExecutionException(s, ex);
            }
            
            var result = JsonSerializer.Deserialize<IEnumerable<ExternalTask>>(s, new JsonSerializerOptions() { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            return result;
        }
    }
}
