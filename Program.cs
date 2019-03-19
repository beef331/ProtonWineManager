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
		static string plLocation =
		System.Environment.GetEnvironmentVariable("HOME") + "/Library.uud",
		jsonlocation = "https://api.steampowered.com/ISteamApps/GetAppList/v1/",
		loadedJson,
		jsonSubString = "\t\"name\": ",
		localJsonPath = AppDomain.CurrentDomain.BaseDirectory + "/Games.json";
		static List<string> directories = new List<string>();
		static Dictionary<string, string> idToPath = new Dictionary<string, string>(), idToName = new Dictionary<string, string>(), commandDesc = new Dictionary<string, string>();
		static string[] keys, commandKeys;
		static void Main(string[] args)
		{
			Init();
			GetLibrary();
			FetchGamesFromJson();
			WineManager();
		}
		static void Init()
		{
			commandDesc.Add("-h", "Displays all commands and their actions");
			commandDesc.Add("-c", "Clears Gamesjson");
			commandDesc.Add("-cls", "Clears Screen");
			commandDesc.Add("-q", "Quits application");
			System.Environment.SetEnvironmentVariable("PWD", AppDomain.CurrentDomain.BaseDirectory);
			commandKeys = commandDesc.Keys.ToArray();
		}
		static void FetchGamesFromJson()
		{
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
						string temp = loadedJson.Split(string.Format(" {0},", gameID))[1].Split("\n")[1];
						temp = temp.Replace(jsonSubString, "").Replace("\"", "");
						idToName.Add(gameID, temp);
					}
				}
			}
			keys = idToName.Keys.ToArray();
		}
		static void ClearCache()
		{
			File.Delete(localJsonPath);
			idToName.Clear();
			idToPath.Clear();
			FetchGamesFromJson();
		}
		static void WriteColoredConsole(string content, ConsoleColor color)
		{
			ConsoleColor temp = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(content);
			Console.ForegroundColor = temp;

		}
		static void GetLibrary()
		{
			if (File.Exists(plLocation))
			{
				using (StreamReader sr = new StreamReader(File.OpenRead(plLocation)))
				{
					while (!sr.EndOfStream)
					{
						directories.Add(sr.ReadLine().Trim());
					}
				}
				if (directories.Count == 0)
				{
					WriteColoredConsole("Please populate your Library.uud file.", ConsoleColor.Red);
					Console.ReadKey();
					System.Environment.Exit(0);
				}
			}
			else
			{
				WriteColoredConsole(plLocation + " not found.", ConsoleColor.Red);
				Console.ReadKey();
				System.Environment.Exit(0);
			}
		}
		static void WineManager()
		{
			WriteColoredConsole("Type a command after the number you want to use", ConsoleColor.Green);
			for (int i = 0; i < keys.Length; i++)
			{
				WriteColoredConsole(string.Format("[{0,-3}] : {1} / APP ID : {2}", i, idToName[keys[i]], keys[i]), ConsoleColor.Yellow);
			}
			string response = Console.ReadLine();
			if (commandKeys.Contains(response))
			{
				if (response.Equals("-h"))
				{
					for (int i = 0; i < commandKeys.Length; i++)
					{
						WriteColoredConsole(commandKeys[i] + "\t" + commandDesc[commandKeys[i]], ConsoleColor.White);
					}
				}
				if (response.Equals("-c"))
				{
					ClearCache();
				}
				if (response.Equals("-q"))
				{
					System.Environment.Exit(0);
				}
				if(response.Equals("-cls")){
					Process.Start("reset");
				}
				WineManager();
			}
			else if (!response.Contains(" "))
			{
				CommandError();
				WineManager();
			}
			try
			{
				string[] splitResponse = response.Split(" ");
				WriteColoredConsole(string.Format("{1} Selected at : {0}", idToPath[keys[int.Parse(splitResponse[0])]], idToName[keys[int.Parse(splitResponse[0])]]), ConsoleColor.Blue);
				Process process = new Process();
				process.StartInfo.FileName = splitResponse[1];
				process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
				process.StartInfo.ArgumentList.Add("2>&1 >/dev/null");
				for (int i = 2; i < splitResponse.Length; i++)
				{
					process.StartInfo.ArgumentList.Add(splitResponse[i]);
				}
				process.StartInfo.EnvironmentVariables.Add("WINEPREFIX", idToPath[keys[int.Parse(splitResponse[0])]]);
				process.Start();
				WineManager();
			}
			catch (Exception e)
			{
				WriteColoredConsole(e.ToString(), ConsoleColor.Magenta);
				CommandError();
				WineManager();
			}
		}
		static void CommandError()
		{
			WriteColoredConsole("Please check the command and try again.", ConsoleColor.Red);
			Console.ReadKey();
		}
	}
}
