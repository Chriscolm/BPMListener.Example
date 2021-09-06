using BPMListener.Example.Exceptions;
using BPMListener.Example.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BPMListener.Example.Requests
{
    public class FetchAndLockRequest : Request
    {
        public async Task<IEnumerable<ExternalTask>> PostAsync(string workerId, string topic, int lockDuration)
        {
            var body = new
            {
                workerId,
                maxTasks = 1,
                topics = new[] { new { topicName = topic, lockDuration } }
            };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            using var http = GetHttpClient();
            var response = await http.PostAsync("external-task/fetchAndLock", content);            
            var s = await response.Content.ReadAsStringAsync();
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new ExecutionException(s, ex);
            }
            var result = JsonSerializer.Deserialize<IEnumerable<ExternalTask>>(s, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            return result;
        }
    }
}
