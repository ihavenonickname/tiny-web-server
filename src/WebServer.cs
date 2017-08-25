using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using static TinyWebServer.Response;

namespace TinyWebServer
{
    public class WebServer
    {
        private readonly IPAddress _host;
        private readonly int _port;
        private readonly Dictionary<StatusCode, string> _errorPage = new Dictionary<StatusCode, string>();
        private readonly Dictionary<string, Action<Request, Response>> _callbacks = new Dictionary<string, Action<Request, Response>>();
        private Action<Request, Response> _fallback;

        public WebServer(string host, int port)
        {
            _host = IPAddress.Parse(host);
            _port = port;

            _errorPage[StatusCode.InternalServerError] = Path.Combine("static", "internal-server-error.html");
            _errorPage[StatusCode.NotFound] = Path.Combine("static", "page-not-found.html");
            _errorPage[StatusCode.BadRequest] = Path.Combine("static", "bad-request.html");
        }

        public void Register(string endpoint, Action<Request, Response> callback)
        {
            _callbacks[endpoint] = callback;
        }

        public void RegisterFallback(Action<Request, Response> fallback)
        {
            _fallback = fallback;
        }

        public async Task RunAsync(CancellationToken canToken)
        {
            TcpListener listener = null;

            try
            {
                listener = new TcpListener(_host, _port);

                listener.Start();

                while (!canToken.IsCancellationRequested)
                {
                    await AcceptConnectionAsync(listener);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                listener?.Stop();
            }
        }

        private async Task AcceptConnectionAsync(TcpListener listener)
        {
            var client = await listener.AcceptTcpClientAsync();

            if (client == null)
            {
                return;
            }

            using (var reader = new StreamReader(client.GetStream()))
            using (var writer = new StreamWriter(client.GetStream()))
            {
                var response = await ProcessRequest(await ReadAsync(reader));

                await writer.WriteAsync(response.ToString());
            }

            client.Close();
        }

        private async Task<string> ReadAsync(StreamReader stream)
        {
            var bufferSize = 1024;
            var buffer = new char[bufferSize];
            var sb = new StringBuilder();

            while (true)
            {
                var nReadBytes = await stream.ReadAsync(buffer, 0, bufferSize);

                for (var i = 0; i < nReadBytes; i++)
                {
                    sb.Append(buffer[i]);
                }

                if (nReadBytes != bufferSize)
                {
                    return sb.ToString();
                }
            }
        }

        private async Task<Response> ProcessRequest(string requestText)
        {
            Request request = null;
            var response = new Response();

            try
            {
                request = new Request(requestText);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);

                response.Code = StatusCode.BadRequest;
                response.Content = await File.ReadAllTextAsync(_errorPage[StatusCode.BadRequest]);

                return response;
            }

            try
            {
                if (_callbacks.ContainsKey(request.Endpoint))
                {
                    _callbacks[request.Endpoint](request, response);
                }
                else
                {
                    response.Code = StatusCode.NotFound;
                    response.Content = await File.ReadAllTextAsync(_errorPage[StatusCode.NotFound]);

                    _fallback?.Invoke(request, response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                response.Code = StatusCode.InternalServerError;
                response.Content = await File.ReadAllTextAsync(_errorPage[StatusCode.InternalServerError]);
            }

            return response;
        }
    }
}
