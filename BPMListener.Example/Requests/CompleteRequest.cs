using BPMListener.Example.Exceptions;
using BPMListener.Example.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BPMListener.Example.Requests
{
    public class CompleteRequest : Request
    {
        public async Task PostAsync(ExternalTask task, IDictionary<string, Variable> variables)
        {
            var vars = variables ?? new Dictionary<string, Variable>();
            if (!vars.ContainsKey("hasError"))
            {
                // für ConditionalFlow/DefaultFlow muss "hasError" in beiden möglichen Ausführungszweigen vorhanden sein
                vars.Add("hasError", new Variable() { Value = "false" });
            }
            var body = new
            {
                workerId = task.WorkerId,
                variables = vars                
            };
            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions() { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var http = GetHttpClient();
            var response = await http.PostAsync($"external-task/{task.Id}/complete", content);
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
