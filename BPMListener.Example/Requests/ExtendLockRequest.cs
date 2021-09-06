using BPMListener.Example.Exceptions;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BPMListener.Example.Requests
{
    public class ExtendLockRequest : Request
    {
        public async Task PostAsync(string taskId, string workerId, int newDurationInMs)
        {            
            var body = new
            {
                workerId,
                newDuration = newDurationInMs
            };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            using var http = GetHttpClient();
            var response = await http.PostAsync($"external-task/{taskId}/extendLock", content);
            var s = await response.Content.ReadAsStringAsync();
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new ExecutionException(s, ex);
            }                     
        }
    }
}
