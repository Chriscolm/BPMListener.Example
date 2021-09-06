using System;
using System.Net.Http;

namespace BPMListener.Example.Requests
{
    public abstract class Request
    {
        protected string BaseUrl => "http://localhost:8080/engine-rest/";
        protected HttpClient GetHttpClient()
        {
            var http = new HttpClient()
            {
                BaseAddress = new Uri(BaseUrl)
            };
            return http;
        }
    }
}
