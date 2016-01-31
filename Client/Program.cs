using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient("127.0.0.1", 6969);
            Console.WriteLine("[Trying to connect to chat...]");
            NetworkStream nStream = client.GetStream();
            var ID = Convert.ToString(new Random((int)DateTime.Now.ToBinary()).Next(0x1111, 0xffff), 16).PadLeft(4, '0');
            Console.WriteLine("[Connected as: " + ID + "]");
            byte[] message = new byte[1];
            do
            {
                message = Encoding.Unicode.GetBytes(ID + ":" + Console.ReadLine());
                nStream.Write(message, 0, message.Length);

                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int newData = nStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                Console.WriteLine(Encoding.Unicode.GetString(bytesToRead, 0, newData));

            } while (Encoding.Unicode.GetString(message).Substring(5) != ":quit");
            client.Close();
            Console.ReadKey();
        }
    }
}
