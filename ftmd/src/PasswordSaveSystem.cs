using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace ftmd
{
    public static class SaveSystem
    {
        public static void Savedata(FtmdData data)
        {
            string path = "";
            if(IsLinux)
                path = "/etc/ftmd/ftmd.conf";
            else
                path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\ftmd\ftmd.conf";
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, data);
            stream.Close();
        }

        public static FtmdData LoadData()
        {
            try
            {
                string path = "";
                if(IsLinux)
                    path = "/etc/ftmd/ftmd.conf";
                else
                    path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\ftmd\ftmd.conf";
                if (File.Exists(path))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    FileStream stream = new FileStream(path, FileMode.Open);
                    FtmdData data = formatter.Deserialize(stream) as FtmdData;
                    stream.Close();
                    return data;
                }
                else
                {
                    return null;
                } 
            }  
            catch
            {
                return null;
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

    [Serializable]
    public class FtmdData
    {
        public string password;
        public FtmdData(string pass)
        {
            this.password = pass;
        }
    }
}

