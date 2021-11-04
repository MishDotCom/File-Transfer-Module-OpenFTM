using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;

namespace ftmd
{
    class Program
    {
        static int port = 84;
        static string password = "qwerty";
        static string address = "127.0.0.1";
        static bool running = false;
        static TcpListener main_listener;
        //args : start stop config
        static void Main(string[] args)
        {
            Console.WriteLine("OpenFTM - ftmd  (C) MishDotCom. All rights reserved.");
            if(args.Length == 1)
            {
                if(args[0].ToLower() == "start" && !running)
                {
                    if(!running)
                    {
                        FtmdData data = SaveSystem.LoadData();
                        if(data != null)
                        {
                            password = data.password;
                            LaunchServer();
                        }
                        else
                        {
                            Console.WriteLine("ftmd: Configuration file not found! Run 'ftmd config' to setup!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("ftmd: Server is already running!");
                    }
                }
                else if(args[0].ToLower() == "stop")
                {
                    if(running)
                    {
                        main_listener.Stop();
                        Console.WriteLine("ftmd: Ftmd server stopped.");
                    }
                    else
                    {
                        Console.WriteLine("ftmd: Server is already stopped!");
                    }
                }
                else if(args[0].ToLower() == "config")
                {
                    AuxCommands.Config();
                }
                else
                {
                    Console.WriteLine("ftmd: Invalid arguments!\n[ftmd] Command list: \nstart - starts server\nstop - stops server\nconfig - runs setup\n");
                }
            }
            else
            {
                Console.WriteLine("ftmd: Invalid arguments!\n[ftmd] Command list: \nstart - starts server\nstop - stops server\nconfig - runs setup\n");
            }
        }
        static void LaunchServer()
        {
            try
            {
                Console.WriteLine($"ftmd: Starting ftmd server...");
                address = getCurrentAddress(); //REMOVE FOR DEBUG
                main_listener = new TcpListener(IPAddress.Parse(address), port);
                main_listener.Start();
                Console.WriteLine($"ftmd: Begin allow remote connections on {main_listener.LocalEndpoint}...\n");
                running = true;
                while(true)
                {
                    TcpClient client = main_listener.AcceptTcpClient();
                    Console.WriteLine($"ftmd: [MainListener] : Connection Accepted : {client.Client.RemoteEndPoint}. Redirecting...");
                    RedirectConnection(client);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"ftmd: {ex.Message}");
            }
        }

        static void RedirectConnection(TcpClient client)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int k = client.Client.Receive(buffer);
                string data = constructString(buffer, k); //data = password
                if(data == password)
                {
                    //successfull
                    Console.WriteLine($"ftmd: Client {client.Client.RemoteEndPoint} successfully logged in...");
                    Stream str = client.GetStream();
                    byte[] bfr = new byte[1024];
                    ASCIIEncoding enc = new ASCIIEncoding();
                    bfr = enc.GetBytes("log-on");
                    str.Write(bfr, 0, bfr.Length);
                    ListenForFile(client);
                }
                else
                {
                    Console.WriteLine($"ftmd: Client {client.Client.RemoteEndPoint} sent wrong password. Terminating connection..\n");
                    Stream str = client.GetStream();
                    byte[] bfr_ = new byte[1024];
                    ASCIIEncoding enc = new ASCIIEncoding();
                    bfr_ = enc.GetBytes("force-off");
                    str.Write(bfr_, 0, bfr_.Length);
                }
            }
            catch{}
        }
        private static string constructString(byte[] buffer,int chK)
        {
            string recStr = "";
            for(int i = 0; i < chK; i++)
                recStr = recStr + Convert.ToChar(buffer[i]);
            return recStr;
        }

        public static void ListenForFile(TcpClient client)
        {
            Thread th = new Thread(() => {
                try
                {
                    byte[] buffer = new byte[1024];
                    int k = client.Client.Receive(buffer);
                    string data = constructString(buffer, k);  //data = file_name(local_path):destination_path:file_size
                    string[] keys = data.Split('|',3);
                    if(keys.Length == 3)
                    {
                        string file_name = keys[0];
                        string file_location = keys[1];
                        long file_size = long.Parse(keys[2]);
                        //-----------------
                        // responses : invalid_dest_path ;; green_light
                        if(Directory.Exists(file_location))
                        {
                            //send green light
                            Stream str = client.GetStream();
                            byte[] bfr = new byte[1024];
                            ASCIIEncoding enc = new ASCIIEncoding();
                            bfr = enc.GetBytes("green_light");
                            str.Write(bfr, 0, bfr.Length);
                            byte[] file_buffer = new byte[file_size + 8*4096];
                            int k_bfr = client.Client.Receive(file_buffer);
                            string dest_file_path = file_location + "/" + file_name;
                            if(!IsLinux)
                                dest_file_path = file_location + @"\" + file_name;
                            if(!File.Exists(dest_file_path))
                            {
                                using(FileStream sw = File.Create(dest_file_path))
                                {
                                    sw.Write(file_buffer);
                                }
                                Console.WriteLine($"ftmd: Done saving file [{file_name}] from {client.Client.RemoteEndPoint} at {dest_file_path}.\n");
                                byte[] bfr_ = new byte[1024];
                                bfr_ = enc.GetBytes("done");
                                str.Write(bfr_, 0, bfr_.Length);
                                client.Dispose();
                            }
                            else
                            {
                                byte[] bfr_ = new byte[1024];
                                bfr = enc.GetBytes("file_exists");
                                str.Write(bfr_, 0, bfr_.Length);
                                Console.WriteLine($"ftmd: File [{file_name}] from {client.Client.RemoteEndPoint} already exists at {dest_file_path}. Terminating.\n");
                                client.Dispose();
                            }
                        }
                        else
                        {
                            Stream str = client.GetStream();
                            byte[] bfr = new byte[1024];
                            ASCIIEncoding enc = new ASCIIEncoding();
                            bfr = enc.GetBytes("invalid_dest_path");
                            str.Write(bfr, 0, bfr.Length);
                            Console.WriteLine("ftmd: Invalid dest path. Terminating...\n");
                            client.Dispose();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"ftmd: Invalid data received from {client.Client.RemoteEndPoint}. Terminating...\n");
                        client.Dispose();
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"ftmd: {ex.Message}");
                }
            });
            th.Start();
        }

        public static bool IsLinux
        {
            get
            {
                int p = (int) Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        static string getCurrentAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && ip.ToString() != "127.0.1.1")
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
    }
}
