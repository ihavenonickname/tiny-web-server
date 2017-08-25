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

            server.Register("/encerrar", (req, res) =>
            {
                cancelToken.Cancel();

                res.Content = "tchau";
            });

            server.Register("/oie", (req, res) =>
            {
                res.Content = $"Oie!...";
            });

            server.RegisterFallback((req, res) =>
            {
                try
                {
                    Console.WriteLine("------------------");
                    Console.WriteLine(req.OriginalHeader);
                    Console.WriteLine("------------------");
                    Console.WriteLine(req.Method);
                    Console.WriteLine(req.Endpoint);
                    Console.WriteLine(req.Host);
                    Console.WriteLine(req.UserAgent);
                    Console.WriteLine(string.Join(" -- ", req.AcceptLanguage));
                    Console.WriteLine(string.Join(" -- ", req.AcceptEncoding));

                    foreach (var x in req.QueryString)
                    {
                        Console.WriteLine($"   {x.Key} -- {x.Value}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }

                res.Content = "Blz... " + req.Endpoint;
            });

            server.RunAsync(cancelToken.Token).GetAwaiter().GetResult();

            Console.WriteLine("Bye!");
        }
    }
}
