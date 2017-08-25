using System;
using System.ComponentModel;
using System.Text;

namespace TinyWebServer
{
    public class Response
    {
        public enum StatusCode
        {
            [Description("OK")]
            Ok = 200,

            [Description("Bad Request")]
            BadRequest = 400,

            [Description("Not Found")]
            NotFound = 404,

            [Description("Internal Server Error")]
            InternalServerError = 500
        }

        public enum ContentType
        {
            [Description("text/html")]
            TextHtml
        }

        public string Content { get; set; }
        public StatusCode Code { get; set; }
        public ContentType Type { get; set; }
        public DateTime Date { get; set; }

        public Response()
        {
            Content = "";
            Code = StatusCode.Ok;
            Type = ContentType.TextHtml;
            Date = DateTime.UtcNow;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var dateStr = Date.ToString("ddd, dd MMM yyyy hh:mm:ss");

            sb.AppendLine($"HTTP/1.1 {(int)Code} {Code}");
            sb.AppendLine($"Date: {dateStr} GMT");
            sb.AppendLine("Server: gabriel-debian");
            sb.AppendLine($"Last-Modified: {dateStr} GMT");
            sb.AppendLine("Content-Length: " + Content.Length);
            sb.AppendLine("Content-Type: " + Type.ToString());
            sb.AppendLine("Connection: Closed");
            sb.AppendLine();
            sb.Append(Content);

            return sb.ToString();
        }
    }
}
