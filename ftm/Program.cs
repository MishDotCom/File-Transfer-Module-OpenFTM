using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Reflection;

namespace ftm
{
    class Program
    {
        static string addres;
        static int port = 84;
        static string password;
        static string local_path;
        static string dest_path;

        //UI
        //args scheme: ftm ipaddress local_path dest_path
        static void Main(string[] args)
        {
            Console.WriteLine("OpenFTM - ftm (C) MishDotCom. All rights reserved.\n");
            if(args.Length == 3)
            {
                addres = args[0];
                local_path = args[1];
                dest_path = args[2];
                InitiateConversation();
            }
            else
            {
                Console.WriteLine("ftm: Invalid arguments!\n[SYNTAX] : ftm <ip_address> <local_file_path> <remote_destination_directory>\n");
            }
        }

        static void InitiateConversation()
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(addres, port);
                Console.Write($"ftm: Enter {addres}'s password: ");
                password = Console.ReadLine();
                Stream str = client.GetStream();
                byte[] bfr = new byte[1024];
                ASCIIEncoding enc = new ASCIIEncoding();
                bfr = enc.GetBytes(password);
                str.Write(bfr, 0, bfr.Length);
                byte[] buffer = new byte[1024];
                int k = client.Client.Receive(buffer);
                string data = constructString(buffer, k);
                if(data == "log-on")
                {
                    Console.WriteLine("ftm: Successfully authenticated! Sending file...");
                    SendFile(client);
                }
                else
                {
                    Console.WriteLine("ftm: Wrong password! Closing ftm.");
                    client.Dispose();
                    Thread.Sleep(1500);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"error: ftm: {ex.Message}");
            }
        }
        private static string constructString(byte[] buffer,int chK)
        {
            string recStr = "";
            for(int i = 0; i < chK; i++)
                recStr = recStr + Convert.ToChar(buffer[i]);
            return recStr;
        }

        static void SendFile(TcpClient client)
        {
            Thread th = new Thread(() => {
                try
                {
                    byte[] buffer = new byte[1024];
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    buffer = encoding.GetBytes($"{Path.GetFileName(local_path)}|{dest_path}|{new FileInfo(local_path).Length}");
                    Stream str = client.GetStream();
                    str.Write(buffer, 0, buffer.Length);
                    Console.WriteLine("ftm: Sent file data packet...");
                    //receive msg
                    buffer = new byte[1024];
                    int k = client.Client.Receive(buffer);
                    string data = constructString(buffer, k);
                    if(data == "green_light")
                    {
                        Console.WriteLine("ftm: Begin sending file...");
                        buffer = new byte[new FileInfo(local_path).Length + 8*4096];
                        buffer = File.ReadAllBytes(local_path);
                        str.Write(buffer, 0, buffer.Length);
                        //recv
                         buffer = new byte[1024];
                        int _ = client.Client.Receive(buffer);
                        data = constructString(buffer, _);
                        if(data == "done")
                        {
                            Console.WriteLine("ftm: File transfer completed");
                        }
                        else
                        {
                            Console.WriteLine("ftm: File transfer failed. File already exists.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("ftm: Invalid destination path. Closing ftm.");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            th.Start();
        }
    }
}
