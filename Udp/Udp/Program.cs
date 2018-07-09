using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Udp
{
    class Program
    {
        static IPAddress remoteAddress; // host for sending data
        static int remotePort = 2111;
        static int localPort = 2221;
        static string userName;
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Enter your name:");
                userName = Console.ReadLine();
                remoteAddress = IPAddress.Parse("233.233.233.233");
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
            IPEndPoint endPoint = new IPEndPoint(remoteAddress, remotePort);
            try
            {
                while (true)
                {
                    string message = Console.ReadLine();
                    message = String.Format($"{userName} : {message}");
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    sender.Send(data, data.Length, endPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }
        private static void ReceiveMessage()
        {
            UdpClient receiver = new UdpClient(localPort);
            receiver.JoinMulticastGroup(remoteAddress, 11);
            IPEndPoint remoteIP = null;
            string localAddress = GetLocalAddress();
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIP);
                    if (remoteIP.Address.ToString().Equals(localAddress))
                        continue;
                    string mess = Encoding.Unicode.GetString(data);
                    Console.WriteLine(mess);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (remoteIP != null)
                    remoteIP = null;
                receiver.Close();
            }
        }

        private static string GetLocalAddress()
        {
            string localIP = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress item in host.AddressList)
            {
                if(item.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = item.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}