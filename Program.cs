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
        static string plLocation = System.Environment.GetEnvironmentVariable("HOME") + "/Library.uud", jsonlocation = "https://api.steampowered.com/ISteamApps/GetAppList/v1/", loadedJson, jsonSubString="\t\"name\": ";
        static List<string> directories = new List<string>();
        static Dictionary<string, string> idToPath = new Dictionary<string, string>(), idToName = new Dictionary<string, string>(), commandDesc = new Dictionary<string, string>();
        static void Main(string[] args)
        {
            commandDesc.Add("-h", "Displays all commands and their actions");
            commandDesc.Add("-c", "Clears Gamesjson");
            commandDesc.Add("-q", "Quits application");
            string localJsonPath = AppDomain.CurrentDomain.BaseDirectory + "/Games.json";
            System.Environment.SetEnvironmentVariable("PWD", AppDomain.CurrentDomain.BaseDirectory);
            if (File.Exists(plLocation))
            {
                using (StreamReader sr = new StreamReader(File.OpenRead(plLocation)))
                {
                    while (!sr.EndOfStream)
                    {
                        directories.Add(sr.ReadLine().Trim());
                    }
                }
            FetchGames:;
                if (!File.Exists(localJsonPath))
                {
                    loadedJson = new StreamReader(WebRequest.Create(jsonlocation).GetResponse().GetResponseStream()).ReadToEnd();
                    using (StreamWriter sw = new StreamWriter(File.Create(localJsonPath)))
                    {
                        sw.Write(loadedJson);
                    }
                }
                else
                {
                    using (StreamReader sr = new StreamReader(File.OpenRead(localJsonPath)))
                    {
                        loadedJson = sr.ReadToEnd();
                    }
                }
                string gameID = "";
                foreach (string dir in directories)
                {
                    string properDir = dir + "/steamapps/compatdata/";
                    foreach (string subDir in Directory.GetDirectories(properDir))
                    {
                        gameID = subDir.Substring(properDir.Length);
                        if (gameID != "pfx" && !idToName.Keys.Contains(gameID))
                        {
                            idToPath.Add(gameID, subDir + "/pfx");
                            string temp = loadedJson.Split(string.Format(" {0},",gameID))[1].Split("\n")[1];
                            temp = temp.Replace(jsonSubString,"").Replace("\"","");
                            idToName.Add(gameID, temp);
                        }
                    }
                }
                string[] keys = idToName.Keys.ToArray();
            ProtonWineManager:;
                WriteColoredConsole("Type a command after the number you want to use", ConsoleColor.Green);
                for (int i = 0; i < keys.Length; i++)
                {
                    WriteColoredConsole(string.Format("[{0}] : {1} / APP ID : {2}", i, idToName[keys[i]], keys[i]), ConsoleColor.Yellow);
                }
                string response = Console.ReadLine();
                string[] commandKeys = commandDesc.Keys.ToArray();
                if (commandKeys.Contains(response))
                {
                    if (response.Equals("-h"))
                    {
                        for (int i = 0; i < commandKeys.Length; i++)
                        {
                            WriteColoredConsole(commandKeys[i] + "\t" + commandDesc[commandKeys[i]], ConsoleColor.White);
                        }
                        goto ProtonWineManager;
                    }
                    if (response.Equals("-c"))
                    {
                        File.Delete(localJsonPath);
                        idToName.Clear();
                        idToPath.Clear();
                        goto FetchGames;
                    }
                    if (response.Equals("-q"))
                    {
                        return;
                    }
                }
                if (!response.Contains(" "))
                {
                    goto CommandError;
                }
                try
                {
                    string[] splitResponse = response.Split(" ");
                    WriteColoredConsole(string.Format("{1} Selected at : {0}", idToPath[keys[int.Parse(splitResponse[0])]], idToName[keys[int.Parse(splitResponse[0])]]), ConsoleColor.Blue);
                    Process process = new Process();
                    process.StartInfo.FileName = splitResponse[1];
                    process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    for (int i = 2; i < splitResponse.Length; i++)
                    {
                        process.StartInfo.ArgumentList.Add(splitResponse[i]);
                    }
                    process.StartInfo.EnvironmentVariables.Add("WINEPREFIX", idToPath[keys[int.Parse(splitResponse[0])]]);
                    process.Start();
                    goto ProtonWineManager;
                }
                catch (Exception e)
                {
                    WriteColoredConsole(e.ToString(), ConsoleColor.Magenta);
                    goto CommandError;
                }
            CommandError:;
                WriteColoredConsole("Please check the command and try again.", ConsoleColor.Red);
                Console.ReadKey();
                goto ProtonWineManager;
            }
            else
            {
                WriteColoredConsole(plLocation + " not found.", ConsoleColor.Red);
                Console.ReadKey();
            }
        }
        static void WriteColoredConsole(string content, ConsoleColor color)
        {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(content);
            Console.ForegroundColor = temp;

        }
    }
}
