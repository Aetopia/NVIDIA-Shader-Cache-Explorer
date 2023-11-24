using System;
using System.Collections.Generic;
using System.IO;
using Windows.Management.Deployment;
using Windows.ApplicationModel;
using System.Text;

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

    private static List<string> GetPackageFamilyNames()
    {
        List<string> packageFamilyNames = [];
        foreach (Package package in new PackageManager().FindPackages())
            packageFamilyNames.Add(package.Id.FamilyName);
        return packageFamilyNames;
    }

    public static string GetProcess(string path, List<string> packageFamilyNames = null)
    {
        packageFamilyNames ??= GetPackageFamilyNames();
        byte[] bytes = File.ReadAllBytes(path);
        for (int i = 0; i < bytes.Length; i++)
            if (bytes[i] == 0) bytes[i] = (byte)'|';
        string[] content = Encoding.UTF8.GetString(bytes).Split(['|'], StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < content.Length; i++)
        {
            string value = content[i].Trim().ToLower();
            if (packageFamilyNames.Contains(value) || value.EndsWith(".exe"))
                return value;
        }
        return null;
    }

    public static Dictionary<string, string> GetProcesses(string path = null)
    {
        Dictionary<string, string> processes = [];
        List<string> packageFamilyNames = GetPackageFamilyNames();
        string[] paths = Directory.GetFiles(path ??= GetPath(), "*.toc");
        for (int i = 0; i < paths.Length; i++)
            try
            {
                if (new FileInfo(paths[i]).Length == 1024)
                {
                    string process = GetProcess(paths[i], packageFamilyNames);
                    if (!processes.ContainsValue(process) && !string.IsNullOrEmpty(process))
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