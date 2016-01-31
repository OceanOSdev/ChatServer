using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var server = new ChatServer();
                server.Start();
                Console.WriteLine("Listening....");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex, "Main");
            }
        }
    }

    internal class ChatServer
    {
        private readonly TcpListener _listener = new TcpListener(IPAddress.Any, 6969);
        private readonly Dictionary<string, LocalClient> _clients = new Dictionary<string, LocalClient>();

        public async void Start()
        {
            try
            {
                _listener.Start();
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Listening...");
                        var client = await _listener.AcceptTcpClientAsync();

                        Console.WriteLine("Client connected");
                        var id = Guid.NewGuid().ToString();
                        _clients.Add(id, new LocalClient(client, ReceivedMessage, ClientClosed, id));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Start() While() '{0}', '{1}'", ex.Message, ex.StackTrace);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex, "Start");
            }
        }

        public void ClientClosed(string id)
        {
            Console.WriteLine("Client {0} disconnected", id);
            var client = _clients[id];
            _clients.Remove(id);
            client.Close();
            Console.WriteLine("Client {0} Closed", id);
            ReceivedMessage("Client disconnected", id);
        }

        public void ReceivedMessage(string message, string id)
        {
            try
            {
                var clientsToRemove = new List<string>();
                foreach (var client in _clients)
                {
                    if (client.Value != null && client.Value.Client.Connected)
                    {
                        if (client.Key != id)
                            client.Value.Send(id + "> " + message);
                    }
                    else
                        clientsToRemove.Add(client.Key);
                }

                foreach (var client in clientsToRemove)
                    _clients.Remove(client);
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex, "ReceivedMessage");
            }
        }

        internal class LocalClient
        {
            private readonly TcpClient _client;
            public TcpClient Client { get { return _client; } }

            private readonly Action<string> _closedCallback;
            private readonly Action<string, string> _recvCallback;

            private readonly StreamReader _reader;
            private readonly StreamWriter _writer;

            private readonly string _id;
            public string Id { get { return _id; } }

            public LocalClient(TcpClient client, Action<string, string> recvCallback, Action<string> closedCallback, string id)
            {
                _closedCallback = closedCallback;
                try
                {
                    _client = client;
                    _recvCallback = recvCallback;
                    _id = id;

                    _reader = new StreamReader(_client.GetStream());
                    _writer = new StreamWriter(_client.GetStream()) { AutoFlush = true };

                    StartReceive();

                    Console.WriteLine("Local client {0} receiving...", id);
                }
                catch (Exception ex)
                {
                    Logger.PrintException(ex, "LocalClient");
                }
            }

            public async void StartReceive()
            {
                try
                {
                    while (true)
                    {
                        var message = await _reader.ReadLineAsync();

                        if (String.IsNullOrEmpty(message))
                        {
                            await Task.Run(() => _closedCallback(_id));
                            return;
                        }

                        _recvCallback(message, _id);
                        Console.WriteLine("{0} > {1}", _id, message);
                    }
                }
                catch (Exception ex)
                {
                    Logger.PrintException(ex, "StartReceive");
                }
            }

            public void Close()
            {
                if (_reader != null)
                    _reader.Dispose();
            }

            public async void Send(string message)
            {
                try
                {
                    if (String.IsNullOrEmpty(message)) return;

                    await _writer.WriteLineAsync(message);
                }
                catch (Exception ex)
                {
                    Logger.PrintException(ex, "Send");
                }
            }
        }
    }

    public static class Logger
    {
        public static void PrintException(Exception ex, string methodName)
        {
            Console.WriteLine("{0}: \n '{1}' \n '{2}'", methodName, ex.Message, ex.StackTrace);
        }
    }

}
