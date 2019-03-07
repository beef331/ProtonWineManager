using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
namespace ProtonWineManager
{
    class Program
    {
        static string plLocation = System.Environment.GetEnvironmentVariable("HOME") + "/Library.uud",dbURL = "https://steamdb.info/app/{0}/", cache = "/CachedGames.uud";
        static List<string>  directories = new List<string>();
        static Dictionary<string,string> idToPath = new Dictionary<string, string>(),idToName = new Dictionary<string, string>();
        static void Main(string[] args)
        {
            cache = AppDomain.CurrentDomain.BaseDirectory + cache;
            System.Environment.SetEnvironmentVariable("PWD",AppDomain.CurrentDomain.BaseDirectory);
            if(File.Exists(plLocation)){
            using (StreamReader sr = new StreamReader(File.OpenRead(plLocation)))
            {
                while (!sr.EndOfStream)
                {
                    directories.Add(sr.ReadLine().Trim());
                }
            }
            if(File.Exists(cache)){
                using(StreamReader sr = new StreamReader(File.OpenRead(cache))){
                    while(!sr.EndOfStream){
                        string[] data = sr.ReadLine().Split("^");
                        if(Directory.Exists(data[2])){
                            idToName.Add(data[0],data[1]);
                            idToPath.Add(data[0],data[2]);
                        }
                    }
                }
            }
            string gameID = "";
            foreach (string dir in directories)
            {
                string properDir = dir + "/steamapps/compatdata/";
                foreach (string subDir in Directory.GetDirectories(properDir))
                {
                    gameID = subDir.Substring(properDir.Length);
                    if(gameID != "pfx" && !idToName.Keys.Contains(gameID)){
                        idToPath.Add(gameID,subDir + "/pfx");
                        using(StreamReader sr = new StreamReader(WebRequest.Create(string.Format(dbURL,gameID)).GetResponse().GetResponseStream())){
                            for(int i = 0 ;i <5;i++){
                                sr.ReadLine();
                            }
                            idToName.Add(gameID,sr.ReadLine().Split("title>")[1].Split('·')[0]);
                        }
                    }
                }
            }
            using(StreamWriter sw = new StreamWriter(File.OpenWrite(cache))){
                string[] ids = idToName.Keys.ToArray();
                for(int i =0; i < ids.Length;i++){
                    sw.WriteLine(string.Format("{0}^{1}^{2}",ids[i],idToName[ids[i]],idToPath[ids[i]]));
                }
            }
            string[] keys = idToName.Keys.ToArray();
            ProtonWineManager:;
            WriteColoredConsole("Type a command after the number you want to use",ConsoleColor.Green);
            for(int i =0;i < keys.Length;i++){
                WriteColoredConsole(string.Format("[{0}] : {1} / APP ID : {2}",i,idToName[keys[i]],keys[i]),ConsoleColor.Yellow);
            }
            string response = Console.ReadLine();
            if(!response.Contains(" ")){
                goto CommandError;
            }
            try{
                string[] splitResponse = response.Split(" ");
                WriteColoredConsole(string.Format("{1} Selected at : {0}",idToPath[keys[int.Parse(splitResponse[0])]],idToName[keys[int.Parse(splitResponse[0])]]),ConsoleColor.Blue);
                Process process = new Process();
                process.StartInfo.FileName = splitResponse[1];
                process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                for(int i =2; i < splitResponse.Length;i++){
                    process.StartInfo.ArgumentList.Add(splitResponse[i]);
                }
                process.StartInfo.EnvironmentVariables.Add("WINEPREFIX",idToPath[keys[int.Parse(splitResponse[0])]]);
                process.Start();
                goto ProtonWineManager;
            }catch(Exception e){
                WriteColoredConsole(e.ToString(),ConsoleColor.Magenta);
                goto CommandError;
            }
            CommandError:;
                WriteColoredConsole("Please check the command and try again.",ConsoleColor.Red);
                Console.ReadKey();
            goto ProtonWineManager;
            }
            else{
                WriteColoredConsole(plLocation + " not found.",ConsoleColor.Red);
                Console.ReadKey();
            }
        }
        static void WriteColoredConsole(string content, ConsoleColor color){
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(content);
            Console.ForegroundColor = temp;

        }
    }
}
