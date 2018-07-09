using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Udp
{
    class Program
    {
        static string remoteAddress;
        static int remotePort;
        static int localPort;
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Enter address for connecting");
                remoteAddress = Console.ReadLine();
                Console.WriteLine("Enter port for listening");
                localPort = Int32.Parse(Console.ReadLine());
                Console.WriteLine("Enter port for connecting");
                remotePort = Int32.Parse(Console.ReadLine());

                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
                SendMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void SendMessage()
        {
            UdpClient sender = new UdpClient();
            try
            {
                while (true)
                {
                    string message = Console.ReadLine();
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    sender.Send(data, data.Length, remoteAddress, remotePort);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender?.Close();
            }
        }

        private static void ReceiveMessage()
        {
            UdpClient receiver = new UdpClient(localPort);
            IPEndPoint remoteIp = null;
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIp);
                    string mess = Encoding.Unicode.GetString(data);
                    Console.WriteLine($"Message: {mess}"); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                receiver?.Close();
            }
        }
    }
}