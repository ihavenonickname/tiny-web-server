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

            server.PublicFolder = "static";

            server.Register("/encerrar", (req, res) =>
            {
                cancelToken.Cancel();

                res.Content = "tchau";
            });

            server.Register("/hello", (req, res) =>
            {
                res.Content = $"Hello there...!";
            });

            server.Register("/error", (req, res) =>
            {
                throw new Exception("dead monkey");
            });

            server.RunAsync(cancelToken.Token).GetAwaiter().GetResult();

            Console.WriteLine("Bye!");
        }
    }
}
