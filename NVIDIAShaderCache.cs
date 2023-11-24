using System;
using System.Collections.Generic;
using System.IO;

class NVIDIAShaderCache
{
    public static string GetPath()
    {
        string path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%Low\NVIDIA\PerDriverVersion\DXCache");
        if (!Directory.Exists(path))
            if (!Directory.Exists(path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\NVIDIA\DXCache")))
                return "";
        return path;
    }


    public static Dictionary<string, string> GetProcesses(string path = null)
    {
        Dictionary<string, string> processes = [];
        string[] paths = Directory.GetFiles(path ??= GetPath(), "*.toc");
        for (int i = 0; i < paths.Length; i++)
            try
            {
                string process = "";
                if (new FileInfo(paths[i]).Length == 1024)
                {
                    byte[] bytes = File.ReadAllBytes(paths[i]);
                    for (int j = 88; j < bytes.Length; j++)
                    {
                        if (bytes[j] == 0) break;
                        process += (char)bytes[j];
                    }
                    if (!processes.ContainsValue(process))
                    {
                        string[] substrings = Path.GetFileNameWithoutExtension(paths[i]).Split('_');
                        processes.Add($"{substrings[0]}_{substrings[1]}_{substrings[2]}", process);
                    }
                }
            }
            catch (IOException) { }
        return processes;
    }

    public static Dictionary<string, long> GetShaderCacheSizes(Dictionary<string, string> processes = null, string path = null)
    {
        Dictionary<string, long> sizes = [];
        foreach (KeyValuePair<string, string> keyValuePair in processes ?? GetProcesses())
        {
            long bytes = 0;
            foreach (string e in Directory.GetFiles(path ??= GetPath(), $"{keyValuePair.Key}*"))
                bytes += new FileInfo(e).Length;
            sizes.Add(keyValuePair.Key, bytes);
        }
        return sizes;
    }

}