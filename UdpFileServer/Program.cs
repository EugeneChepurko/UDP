using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml.Serialization;

namespace UdpFileServer
{
    public class Program
    {
        [Serializable]
        public class FileDetails
        {
            public string fileType = "";
            public long fileSize = 0;
        }

        private static FileDetails details = new FileDetails();
        private static IPAddress remoteIpAddress;
        private const int PORT = 5002;
        private static UdpClient client = new UdpClient();
        private static IPEndPoint endPoint;
        private static FileStream fileStream;

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Enter IP remote host");
                remoteIpAddress = IPAddress.Parse(Console.ReadLine().ToString());
                endPoint = new IPEndPoint(remoteIpAddress, PORT);

                Console.WriteLine("Enter the path and name file with extention");
                fileStream = new FileStream(Console.ReadLine().ToString(), FileMode.Open, FileAccess.Read);

                if (fileStream.Length > 8192)
                {
                    Console.WriteLine("Google is a deceiver!");
                    client.Close();
                    fileStream.Close();
                    return;
                }

                SendFileInfo();
                Thread.Sleep(2000);
                SendFileData();
                HistoryOfFiles historyOfFiles = new HistoryOfFiles();
                historyOfFiles.Add(fileStream, remoteIpAddress);
                historyOfFiles.Show();
                //History(fileStream, remoteIpAddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static void SendFileData()
        {
            byte[] data = new byte[fileStream.Length];
            fileStream.Read(data, 0, Convert.ToInt32(fileStream.Length));

            Console.WriteLine("File sending...");
            try
            {
                client.Send(data, data.Length, endPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                fileStream?.Close();
                client.Close();
            }
            Console.WriteLine("File sent!");
        }
        private static void SendFileInfo()
        {
            details.fileType = fileStream.Name.Substring((int)fileStream.Name.Length - 3, 3);
            details.fileSize = fileStream.Length;

            XmlSerializer serializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream memory = new MemoryStream();
            serializer.Serialize(memory, details);
            memory.Position = 0;
            byte[] data = new byte[memory.Length];
            memory.Read(data, 0, Convert.ToInt32(memory.Length));

            Console.WriteLine("Sending file description");

            client.Send(data, data.Length, endPoint);
            memory.Close();
        }
        public class HistoryOfFiles
        {
            private List<string> FileList = new List<string>();
            private List<string> RemoteIP = new List<string>();
            public void Add(FileStream file, IPAddress remoteIp)
            {
                FileList.Add(file.Name);
                RemoteIP.Add(remoteIp.ToString());
            }
            public void Show()
            {
                Console.WriteLine("File sending history");
                foreach (string files in FileList)
                {
                    foreach (string remoteIp in RemoteIP)
                    {
                        Console.WriteLine("File " + files + " has been sent " + remoteIp);
                    }
                }
            }
        }
        private static void History(FileStream file, IPAddress remoteIP)
        {
            Console.WriteLine("File sending history");
            List<string> FileList = new List<string>();
            FileList.Add(file.Name);
            foreach (string files in FileList)
            {
                Console.WriteLine("File " + files + " has been sent " + remoteIP);
            }
        }
    }
}