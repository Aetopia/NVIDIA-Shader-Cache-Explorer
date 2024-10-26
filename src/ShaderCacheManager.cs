using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

file struct SynchronizationContextRemover : INotifyCompletion
{
    internal readonly bool IsCompleted => SynchronizationContext.Current == null;

    internal readonly SynchronizationContextRemover GetAwaiter() => this;

    internal readonly void GetResult() { }

    public readonly void OnCompleted(Action continuation)
    {
        var syncContext = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(null);
            continuation();
        }
        finally { SynchronizationContext.SetSynchronizationContext(syncContext); }
    }
}

interface IShader
{
    string Name { get; }

    string Size { get; }

    IEnumerable<string> Files { get; }
}

file struct Shader(string name, string size, IEnumerable<string> files) : IShader
{
    public readonly string Name => name;

    public readonly string Size => size;

    public readonly IEnumerable<string> Files => files;
}

static class ShaderCacheManager
{
    static readonly IEnumerable<string> Paths = [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"AppData\LocalLow\NVIDIA\PerDriverVersion"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NVIDIA"),
        ];

    static readonly string GLCache = Path.Combine(Paths.ElementAt(1), "GLCache");

    static readonly IEnumerable<string> Units = ["B", "KB", "MB", "GB"];


    static int GetFileSize(string path)
    {
        NativeMethods.GetFileAttributesEx(path, 0, out WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);
        return (lpFileInformation.nFileSizeHigh << 32) | lpFileInformation.nFileSizeLow;
    }

    static string FormatFileSize(int size)
    {
        int index = 0;
        while (size >= 1024)
        {
            size /= 1024;
            ++index;
        }
        return string.Format($"{size:0.#} {Units.ElementAt(index)}");
    }

    static async Task<string> ReadAllTextAsync(string path)
    {
        try
        {
            using var file = File.OpenRead(path);
            using StreamReader stream = new(file);
            return await stream.ReadToEndAsync();
        }
        catch (IOException) { return string.Empty; }
    }

    static Dictionary<string, List<string>> GetCache()
    {
        Dictionary<string, List<string>> shaders = [];
        foreach (var path in Paths)
            foreach (var fileName in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                var shader = string.Join("_", Path.GetFileNameWithoutExtension(fileName).Split('_').Take(3));
                if (!shaders.ContainsKey(shader)) shaders.Add(shader, []);
                if (!shaders[shader].Contains(fileName)) shaders[shader].Add(fileName);
            }

        return shaders;
    }

    static async Task<Dictionary<string, string>> GetAppsAsync(Dictionary<string, List<string>> cache)
    {
        Dictionary<string, string> apps = [];
        foreach (var shader in cache)
            foreach (var path in shader.Value.Where(path => path.ToLowerInvariant().EndsWith(".toc")))
            {
                var content = (await ReadAllTextAsync(path)).Split([(char)0], StringSplitOptions.RemoveEmptyEntries);
                if (!content.Any() || !content[0].Equals("DXDC")) continue;

                for (int i = 0; i < content.Length; i++)
                {
                    var input = content[i].Trim();
                    if (input.EndsWith(".exe") || input.EndsWith(".com") || Regex.IsMatch(input, @"^[a-zA-Z0-9.-]+_[a-zA-Z0-9]+$"))
                        if (!apps.ContainsKey(input)) apps.Add(input, shader.Key);
                }
            }

        return apps;
    }

    internal static async Task<IEnumerable<IShader>> GetShadersAsync()
    {
        await default(SynchronizationContextRemover);

        var cache = GetCache();
        var apps = await GetAppsAsync(cache);
        List<IShader> shaders = [];

        foreach (var app in apps)
            shaders.Add(new Shader(app.Key, FormatFileSize(cache[app.Value].Select(GetFileSize).Sum()), cache[app.Value]));

        var files = cache.Select(shader => shader.Value).SelectMany(path => path).Where(input => Regex.IsMatch(input, ".*GLCache.*", RegexOptions.IgnoreCase));
        var size = files.Select(GetFileSize).Sum();
        if (size != 0) shaders.Add(new Shader("glcache", FormatFileSize(size), [GLCache]));

        return shaders;
    }
}