using System.Collections.Generic;

namespace TinyWebServer
{
    public class Request
    {
        public string Method { get; }
        public string Endpoint { get; }
        public string HttpVersion { get; }
        public string From { get; }
        public string UserAgent { get; }
        public Dictionary<string, string> UrlParameters { get; }

        private Request(string method, string endpoint, string httpVersion, string from, string userAgent)
        {
            Method = method;
            Endpoint = endpoint;
            HttpVersion = httpVersion;
            From = from;
            UserAgent = userAgent;
            UrlParameters = new Dictionary<string, string>();
        }

        public static Request FromHttpHeader(string text)
        {
            var endpoint = text.Split(' ')[1];

            return new Request("Dummy method", endpoint, "Dummy httpversion", "Dummy from", "dummy useragent");
        }
    }
}
