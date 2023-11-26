using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

class NVIDIAShaderCache
{
    public static List<string> GetPaths()
    {
        List<string> paths = [];
        string path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%Low\NVIDIA\PerDriverVersion\DXCache");
        if (Directory.Exists(path))
            paths.AddRange(Directory.GetFiles(path));
        if (Directory.Exists(path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\NVIDIA\DXCache")))
            paths.AddRange(Directory.GetFiles(path));
        return paths;
    }

    public static Dictionary<string, List<string>> GetProcesses(List<string> paths = null)
    {
        Dictionary<string, List<string>> processes = [];
        foreach (string path in paths ??= GetPaths())
            if (path.ToLower().EndsWith(".toc"))
                try
                {
                    string process = "";
                    string[] content = File.ReadAllText(path).Split([(char)0], StringSplitOptions.RemoveEmptyEntries);
                    if (content[0].StartsWith("DXDC"))
                        for (int i = 0; i < content.Length; i++)
                        {
                            process = content[i].Trim().ToLower();
                            if (Regex.IsMatch(process, @"^[a-zA-Z0-9.-]+_[a-zA-Z0-9]+$") || process.EndsWith(".exe"))
                            {
                                if (!processes.ContainsKey(process) && !string.IsNullOrEmpty(process))
                                    processes.Add(process, []);
                                string[] substrings = Path.GetFileNameWithoutExtension(path).Split('_');
                                try
                                {
                                    string uid = $"{substrings[0]}_{substrings[1]}_{substrings[2]}";
                                    if (!processes[process].Contains(uid))
                                        processes[process].Add(uid);
                                }
                                catch (ArgumentException) { }
                                break;
                            }
                        }
                }
                catch (IOException) { }
        return processes;
    }

    public static Dictionary<string, long> GetSizes(Dictionary<string, List<string>> processes = null, List<string> paths = null)
    {
        paths ??= GetPaths();
        Dictionary<string, long> sizes = [];

        foreach (KeyValuePair<string, List<string>> keyValuePair in processes ?? GetProcesses())
        {
            long buffer = 0;
            foreach (string path in paths)
                foreach (string uid in keyValuePair.Value)
                    if (Path.GetFileNameWithoutExtension(path).StartsWith(uid)) buffer += new FileInfo(path).Length;
            sizes.Add(keyValuePair.Key, buffer);
        }
        return sizes;
    }



}