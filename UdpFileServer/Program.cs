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
        private static List<PersonInfo> FileList = new List<PersonInfo>();

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Enter IP remote host");
                remoteIpAddress = IPAddress.Parse(Console.ReadLine().ToString());
                endPoint = new IPEndPoint(remoteIpAddress, PORT);

                ConsoleKeyInfo keyInfo = new ConsoleKeyInfo();
                PersonInfo personInfo;
                do
                {
                    Console.WriteLine("Enter the path and name file with extention");
                    personInfo = new PersonInfo();
                    fileStream = new FileStream(Console.ReadLine().ToString(), FileMode.Open, FileAccess.Read);

                    if (fileStream.Length > 8192)
                    {
                        Console.WriteLine("Google is a deceiver!");
                        Disconnect();
                        return;
                    }

                    SendFileInfo();
                    Thread.Sleep(1000);
                    SendFileData();

                    personInfo.AddInfoPerson(fileStream, remoteIpAddress, DateTime.Now);

                    personInfo.AddPerson(personInfo);
                    Console.WriteLine("Press Esc to exit or another key for enter the path:");
                    keyInfo = Console.ReadKey(true);
                    //fileStream.Close();  ??
                } while (keyInfo.Key != ConsoleKey.Escape);
                personInfo.ShowList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
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
                Disconnect();
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
        public class PersonInfo
        {
            FileStream myfileStream;
            IPAddress myip;
            DateTime dateTime; 

            internal PersonInfo()
            {
            }
            internal void AddInfoPerson (FileStream file, IPAddress remoteIp, DateTime dateTime)
            {
                this.myfileStream = file;
                this.myip = remoteIp;
                this.dateTime = dateTime;
            }
            internal void AddPerson(PersonInfo person)
            {
                FileList.Add(person);
            }
            private void Show()
            {
                Console.WriteLine("File " + myfileStream.Name + " has been sent " + myip + " in " + this.dateTime);
            }
            internal void ShowList()
            {
                Console.Clear();
                Console.WriteLine("History sending files:");

                foreach (var person in FileList)
                {
                    person.Show();
                }
            }  
        }
        private static void Disconnect()
        {
            fileStream?.Close();
            client.Close();
            Environment.Exit(0);
        }
    }
}