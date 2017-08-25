using System.Linq;
using System.Collections.Generic;
using System;

namespace TinyWebServer
{
    public class Request
    {
        public string Method { get; }
        public string Endpoint { get; }
        public string HttpVersion { get; }
        public string Host { get; }
        public string UserAgent { get; }
        public IEnumerable<string> AcceptLanguage { get; }
        public IEnumerable<string> AcceptEncoding { get; }
        public Dictionary<string, string> QueryString { get; }
        public string OriginalHeader { get; }

        public Request(string text)
        {
            OriginalHeader = text;

            var lines = text.Split('\n');
            var firstLine = lines[0].Split(' ');

            Method = firstLine[0];
            HttpVersion = firstLine[2];

            var endpoint = "";
            var queryString = new Dictionary<string, string>();

            ParseUrl(firstLine[1], ref endpoint, ref queryString);

            Endpoint = endpoint;
            QueryString = queryString;

            var dict = new Dictionary<string, string>();

            foreach (var line in lines.Skip(1))
            {
                var sepIndex = line.IndexOf(':');

                if (sepIndex == -1 || sepIndex == line.Length - 1)
                {
                    continue;
                }

                var key = line.Substring(0, sepIndex);
                var value = line.Substring(sepIndex + 1);

                dict[key.Trim()] = value.Trim();
            }

            if (dict.ContainsKey("User-Agent"))
            {
                UserAgent = dict["User-Agent"];
            }
            else
            {
                UserAgent = "";
            }

            if (dict.ContainsKey("Host"))
            {
                Host = dict["Host"];
            }
            else
            {
                UserAgent = "";
            }

            if (dict.ContainsKey("Accept-Language"))
            {
                AcceptLanguage = dict["Accept-Language"].Split(',').Select(x => x.Trim());
            }
            else
            {
                AcceptLanguage = new string[0];
            }

            if (dict.ContainsKey("Accept-Encoding"))
            {
                AcceptEncoding = dict["Accept-Encoding"].Split(',').Select(x => x.Trim());
            }
            else
            {
                AcceptEncoding = new string[0];
            }
        }

        private void ParseUrl(string url, ref string endpoint, ref Dictionary<string, string> queryString)
        {
            var endEndpoint = url.IndexOf('?');

            if (endEndpoint == -1)
            {
                endpoint = url;

                return;
            }

            endpoint = url.Substring(0, endEndpoint);

            foreach (var str in url.Substring(endEndpoint + 1).Split('&'))
            {
                var items = str.Split('=');

                queryString[items[0]] = items[1];
            }
        }
    }
}
