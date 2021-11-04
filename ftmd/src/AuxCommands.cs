using System;
using System.Threading;
using System.IO;
using System.Reflection;

namespace ftmd
{
    class AuxCommands
    {
        public static void Config()
        {
            Console.WriteLine("Ftmd Server (C) MishDotCom. Config setup...");
            Console.Write("ftmd/conf: Enter a password (strong one:)) for ftmd: ");
            string pass1 = Console.ReadLine();
            Console.Write("ftmd/conf: Confirm password for ftmd: ");
            string pass2 = Console.ReadLine();
            if(pass1 == pass2)
            {
                FtmdData data = new FtmdData(pass1);
                string app_path = AppDomain.CurrentDomain.BaseDirectory + @"\ftmd.exe";
                if(IsLinux)
                    app_path = AppDomain.CurrentDomain.BaseDirectory + "/ftmd";
                string main_dir = "/etc";
                if(!IsLinux){
                    main_dir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                    Directory.CreateDirectory(main_dir + @"\ftmd");
                    SaveSystem.Savedata(data);
                    File.Move(app_path, main_dir + @"\ftmd" + @"\" + Path.GetFileName(app_path));
                    Console.WriteLine("ftmd/conf: Setup finished! You may now start the server!");
                    Console.WriteLine("[note] : ftmd.exe was moved to [C:/ProgramFilesX86/ftmd] run it from there");
                }
                else
                {
                    Directory.CreateDirectory(main_dir + "/ftmd");
                    SaveSystem.Savedata(data);
                    File.Move(app_path, main_dir + "/ftmd" + "/" + Path.GetFileName(app_path));
                    Console.WriteLine("ftmd/conf: Setup finished! You may now start the server!");
                    Console.WriteLine("[note] : ftmd was moved to [/etc/ftmd]. run it from there");
                }

            }
            else
            {
                Console.WriteLine("ftmd/conf: passwords do not match! restarting");
                Thread.Sleep(1000);
                Config();
            }
        }
        static bool IsLinux
        {
            get
            {
                int p = (int) Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
    }
}