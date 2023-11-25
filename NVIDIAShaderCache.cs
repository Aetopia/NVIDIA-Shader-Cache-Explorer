using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;

class NVIDIAShaderCache
{
    public static List<string> GetPaths(bool tableOfContents = false)
    {
        List<string> paths = [];
        string searchPattern = tableOfContents ? "*.toc" : "*";
        string path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%Low\NVIDIA\PerDriverVersion\DXCache");
        if (Directory.Exists(path))
            paths.AddRange(Directory.GetFiles(path, searchPattern));
        if (Directory.Exists(path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\NVIDIA\DXCache")))
            paths.AddRange(Directory.GetFiles(path, searchPattern));
        return paths;
    }

    private static List<string> GetPackageFamilyNames()
    {
        List<string> packageFamilyNames = [];
        using (Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "PowerShell.exe",
                Arguments = "-c (Get-AppxPackage).PackageFamilyName",
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        })
        {
            process.Start();
            process.WaitForExit();
            packageFamilyNames = process.StandardOutput.ReadToEnd()
                 .Split(['\n'], StringSplitOptions.RemoveEmptyEntries)
                 .Select(e => e.Trim().ToLower())
                 .Where(value => !string.IsNullOrEmpty(value))
                 .ToList();
        }
        return packageFamilyNames;
    }

    private static string GetProcess(string path, List<string> packageFamilyNames = null)
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

    public static Dictionary<string, string> GetProcesses()
    {
        Dictionary<string, string> processes = [];
        List<string> packageFamilyNames = GetPackageFamilyNames();
        foreach (string path in GetPaths(true))
            try
            {
                if (new FileInfo(path).Length == 1024)
                {
                    string process = GetProcess(path, packageFamilyNames);
                    if (!processes.ContainsValue(process) && !string.IsNullOrEmpty(process))
                    {
                        string[] substrings = Path.GetFileNameWithoutExtension(path).Split('_');
                        processes.Add($"{substrings[0]}_{substrings[1]}_{substrings[2]}", process);
                    }
                }
            }
            catch (IOException) { }
        return processes;
    }

    public static Dictionary<string, long> GetSizes(Dictionary<string, string> processes = null)
    {
        Dictionary<string, long> sizes = [];
        foreach (KeyValuePair<string, string> keyValuePair in processes ?? GetProcesses())
        {
            long bytes = 0;
            foreach (string path in GetPaths())
                if (path.Contains(keyValuePair.Key)) bytes += new FileInfo(path).Length;
            sizes.Add(keyValuePair.Key, bytes);
        }
        return sizes;
    }
}