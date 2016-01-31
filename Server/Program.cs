using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO;

namespace Server
{
    class Program
    {
        static int connections = -1;
        static InterLockedList<string> chatList = new InterLockedList<string>();
        static StreamWriter sw;
        static void Main(string[] args)
        {
            TcpListener listen = new TcpListener(IPAddress.Any, 6969);
            Console.WriteLine("[Listening...]");
            listen.Start();
            TcpClient client;
            
            while (connections != 0)
            {
                client = listen.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(ThreadProc, client);
                
            }
            listen.Stop();
            Console.ReadKey();
        }

        private static void ThreadProc(object obj)
        {
            var client = (TcpClient)obj;
            Console.WriteLine("[Client Connected]");
            if (connections == -1)
                connections = 1;
            else
                connections += 1;
            NetworkStream nStream = client.GetStream();
            byte[] buffer = new byte[1];
            int data = 1; //nStream.Read(buffer, 0, client.ReceiveBufferSize);
            do
            {
                try
                {
                    buffer = new byte[client.ReceiveBufferSize];
                    data = nStream.Read(buffer, 0, client.ReceiveBufferSize);
                    string stoprint = Encoding.ASCII.GetString(buffer, 0, data);
                    //Console.ForegroundColor = (ConsoleColor)new Random(Convert.ToInt32(stoprint.Substring(0, 4), 16)).Next(15);
                    //Console.Write(stoprint.Substring(0, 4));
                    //Console.ResetColor();
                    Console.WriteLine(stoprint);
                    chatList.add(stoprint);
                    //sw.WriteLine(chatList.ToString());

                    var message = Encoding.ASCII.GetBytes(chatList.ToString());

                    buffer = message;
                    client.ReceiveBufferSize = buffer.Length;
                    nStream.Write(buffer, 0, client.ReceiveBufferSize);
                }
                catch (Exception ex)
                {
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("ERROR");
                    Console.ResetColor();
                    Console.Write($": {ex.Message}]");
                    break;
                }
            } while (!Encoding.ASCII.GetString(buffer, 0, data).Contains(":quit"));

            client.Close();
            connections--;
        }
    }
}
