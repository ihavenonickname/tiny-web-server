using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace TinyWebServer
{
    public class WebServer
    {
        public event Action<Request, Response> OnRequest;

        private readonly IPAddress _host;
        private readonly int _port;
        private readonly string _contentError500;

        public WebServer(string host, int port)
        {
            _host = IPAddress.Parse(host);
            _port = port;
            _contentError500 = "<title>500 Internal Server Error</title><h1>Internal Server Error</h1>";
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
                var response = GetResponse(await ReadAsync(reader));

                await writer.WriteAsync(response.ToString());
            }

            client.Close();
        }

        private async Task<string> ReadAsync(StreamReader stream)
        {
            var bufferSize = 1024;
            var chars = new List<char>();
            var nReadBytes = 0;

            do
            {
                var buffer = new char[bufferSize];

                nReadBytes = await stream.ReadAsync(buffer, 0, bufferSize);

                chars.AddRange(buffer.Take(nReadBytes));
            } while (nReadBytes == bufferSize);

            return string.Join("", chars);
        }

        private Response GetResponse(string text)
        {
            try
            {
                var request = Request.FromHttpHeader(text);
                var response = new Response();

                OnRequest?.Invoke(request, response);

                return response;
            }
            catch (Exception)
            {
                return new Response
                {
                    Code = Response.StatusCode.InternalServerError,
                    Content = _contentError500
                };
            }
        }
    }
}
