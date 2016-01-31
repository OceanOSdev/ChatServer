using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient client;
        NetworkStream nStream;
        string ID;

        public MainWindow()
        {
            InitializeComponent();
            client = new TcpClient("127.0.0.1", 6969);
            lblMessage.Content = "[Trying to connect to chat...]";
            nStream = client.GetStream();
            ID = Convert.ToString(new Random((int)DateTime.Now.ToBinary()).Next(0x1111, 0xffff), 16).PadLeft(4, '0');
            lblMessage.Content = $"[Connected as: {ID}]";
        }

       /* public void bleh()
        {
            //TcpClient client = new TcpClient("127.0.0.1", 6969);
            //Console.WriteLine("[Trying to connect to chat...]");
            //NetworkStream nStream = client.GetStream();
            //var ID = Convert.ToString(new Random((int)DateTime.Now.ToBinary()).Next(0x1111, 0xffff), 16).PadLeft(4, '0');
            //Console.WriteLine("[Connected as: " + ID + "]");
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
        }*/

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (txtIn.Text != ":quit")
            {
                var message = Encoding.ASCII.GetBytes(ID + ":" + txtIn.Text);
                nStream.Write(message, 0, message.Length);

                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                /*var buf = new byte[4];
                int offset = 0;
                while (offset < buf.Length)
                {
                    int len = nStream.Read(buf, offset, buf.Length - offset);
                    offset += len;
                }*/
                int newData = nStream.Read(bytesToRead, 0, bytesToRead.Length);

                txtChat.Text = Encoding.ASCII.GetString(bytesToRead, 0, newData);
                //txtChat.Text = new StreamReader(nStream).ReadToEnd();
                txtIn.Clear();
            }
            else
            {
                nStream.Write(Encoding.ASCII.GetBytes(":quit"), 0, ":quit".Length);
                client.Close();
            }
        }
    }
}
