using System;
using System.Threading;

namespace TinyWebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var server = new WebServer("127.0.0.1", 8081);
            var cancelToken = new CancellationTokenSource();

            server.OnRequest += (req, res) =>
            {
                if (req.Endpoint.ToLower().Contains("encerrar"))
                {
                    cancelToken.Cancel();
                }

                res.Content = $"blz...</br>{req.Endpoint}</br>{req.UserAgent}";
            };

            server.RunAsync(cancelToken.Token).GetAwaiter().GetResult();

            Console.WriteLine("Bye!");
        }
    }
}
