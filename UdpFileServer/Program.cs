using System;
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
                Console.WriteLine("IP удаленного узла");
                remoteIpAddress = IPAddress.Parse(Console.ReadLine().ToString());
                endPoint = new IPEndPoint(remoteIpAddress, PORT);

                Console.WriteLine("Введите путь и имя файла с расширением");
                fileStream = new FileStream(Console.ReadLine().ToString(), FileMode.Open, FileAccess.Read);

                if (fileStream.Length > 8192)
                {
                    Console.WriteLine("Google обманщик ");
                    client.Close();
                    fileStream.Close();
                    return;
                }

                SendFileInfo();
                Thread.Sleep(2000);
                SendFileData();
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

            Console.WriteLine("Отправка файла пошла");
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
                if (fileStream != null)
                    fileStream.Close();
                client.Close();
            }
            Console.WriteLine("Файл отправлен");
            Console.Read();
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

            Console.WriteLine("Отправка описания файла");

            client.Send(data, data.Length, endPoint);
            memory.Close();
        }
    }
}
