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
    static readonly string DXCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"AppData\LocalLow\NVIDIA\PerDriverVersion");
    static readonly string GLCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NVIDIA");

    static string ReadAllText(string path)
    {
        try
        {
            using StreamReader reader = new(File.OpenRead(path));
            return reader.ReadToEnd();
        }
        catch (IOException) { return string.Empty; }
    }

    internal static App[] Get()
    {
        Dictionary<string, App> apps = [];

        if (Directory.Exists(GLCache))
        {
            OpenGL app = new("opengl", GLCache);
            foreach (var path in Directory.EnumerateFiles(GLCache, "*", SearchOption.AllDirectories)) app.Add(path);//app.Add(GLCache);
            if (app.Size is not 0) apps.Add(string.Empty, app);
        }

        if (Directory.Exists(DXCache))
            foreach (var path in Directory.EnumerateFiles(DXCache, "*", SearchOption.AllDirectories))
            {
                var key = string.Join("_", Path.GetFileNameWithoutExtension(path).Split('_').Take(3));
                if (!apps.TryGetValue(key, out var value) && path.EndsWith(".toc", StringComparison.OrdinalIgnoreCase))
                {
                    var collection = ReadAllText(path).Split(['\0'], StringSplitOptions.RemoveEmptyEntries);
                    if (collection.FirstOrDefault() is "DXDC")
                        apps.Add(key, value = new(collection.First(_ => _.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || Regex.IsMatch(_, @"^[a-zA-Z0-9.-]+_[a-zA-Z0-9]+$"))));
                }
                value?.Add(path);
            }

        return [.. apps.Values.OrderByDescending(_ => _.Size)];
    }

}