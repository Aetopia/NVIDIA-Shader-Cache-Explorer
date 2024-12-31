using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class App
{
    internal App(string value) => Name = value;

    readonly List<string> Paths = [];

    internal readonly string Name;

    ulong Value = default;

    internal ulong Size => Value;

    internal void Add(string path)
    {
        Paths.Add(path);
        Unmanaged.GetFileAttributesEx(path, 0, out var lpFileInformation);
        Value += (lpFileInformation.nFileSizeHigh << 32) | lpFileInformation.nFileSizeLow;
    }

    internal virtual void Delete() { foreach (var path in Paths) Unmanaged.DeleteFile(path); }
}

class OpenGL : App
{
    readonly string Path;

    internal OpenGL(string value, string path) : base(value) => Path = path;

    internal override void Delete() { try { Directory.Delete(Path, true); } catch { } }
}

static class Manager
{
    static readonly string DXCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"AppData\LocalLow\NVIDIA\PerDriverVersion\DXCache");
    static readonly string GLCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"NVIDIA\GLCache");

    internal static App[] Get()
    {
        Dictionary<string, App> apps = [];

        if (Directory.Exists(GLCache))
        {
            OpenGL app = new("opengl", GLCache);
            foreach (var path in Directory.EnumerateFiles(GLCache, "*", SearchOption.AllDirectories)) app.Add(path);
            if (app.Size is not 0) apps.Add(string.Empty, app);
        }

        if (Directory.Exists(DXCache))
            foreach (var path in Directory.EnumerateFiles(DXCache, "*", SearchOption.AllDirectories))
            {
                var key = string.Join("_", Path.GetFileNameWithoutExtension(path).Split('_').Take(3));
                if (!apps.TryGetValue(key, out var value) && path.EndsWith(".toc", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var content = File.ReadAllText(path);
                        var _ = false; foreach (var item in content.Split(['\0'], StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!_ && item is "DXDC") _ = true;
                            else if (_ && Regex.IsMatch(item, @"(^.*\.exe$|^[a-zA-Z0-9.-]+_[a-zA-Z0-9]+$)", RegexOptions.IgnoreCase | RegexOptions.Compiled)) { apps.Add(key, value = new(item)); break; }
                        }
                    }
                    catch (IOException) { continue; }
                }
                value?.Add(path);
            }

        return [.. apps.Values];
    }

}