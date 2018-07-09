using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;

namespace UdpFileClient
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
        private static int localPort = 5002;
        private static UdpClient remoteClient = new UdpClient(localPort);
        private static IPEndPoint remotePoint = null;
        private static FileStream fileStream;
        private static byte[] data = new byte[0];

        static void Main(string[] args)
        {
            getFileInfo();
            getFileData();
            Console.Read();
        }

        private static void getFileData()
        {
            Random random = new Random();
            try
            {
                Console.WriteLine("Waiting for file");
                data = remoteClient.Receive(ref remotePoint);
                Console.WriteLine("File is a received. Saving...");
                fileStream = new FileStream(random.Next(Int32.MinValue, Int32.MaxValue).ToString() + "." + details.fileType, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                fileStream.Write(data, 0, data.Length);
                Console.WriteLine("File saved.");
                Console.WriteLine("Opening file.");
                Process.Start(fileStream.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }  
            finally
            {
                fileStream.Close();
                remoteClient.Close();
            }
        }

        private static void getFileInfo()
        {
            try
            {
                Console.WriteLine("Waiting information about file...");
                data = remoteClient.Receive(ref remotePoint);
                Console.WriteLine("Information is a received");
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(FileDetails));
                MemoryStream memory = new MemoryStream();
                memory.Write(data, 0, data.Length);
                memory.Position = 0;

                details = (FileDetails)xmlSerializer.Deserialize(memory);
                Console.WriteLine("File description is a received! Information:");
                Console.WriteLine(details.fileSize + " byte");
                Console.WriteLine("Type " + details.fileType);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}