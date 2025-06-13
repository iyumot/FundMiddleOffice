using Serilog;
using System.Text.Json;

namespace FMO.Plugin;

public static class PluginManager
{
    private static List<IPlugin> Plugins { get; } = new();

    private static List<PluginLoadContext> PluginsLoadContexts { get; } = new();

    static PluginManager()
    {
        LoadAll();
    }

    private static void LoadAll()
    {
        var folder = AppDomain.CurrentDomain.BaseDirectory;

        var directoryInfos = new DirectoryInfo(Path.Combine(folder, "plugins")).GetDirectories();

        foreach (var di in directoryInfos)
        {
            try
            {
                // 先找def.json
                var fi = new FileInfo(Path.Combine(di.FullName, "def.json"));
                if (!fi.Exists) continue;

                using var sr = new StreamReader(fi.FullName);

                var def = JsonSerializer.Deserialize<PluginDefinition>(sr.ReadToEnd());
                if(def is null)
                {
                    Log.Error($"Load Plugin [{di.Name}] Error: 无法解析配置");
                    continue;
                }

                def.Folder = di.FullName;
                var context = new PluginLoadContext(def);

                PluginsLoadContexts.Add(context);
                Plugins.AddRange(context.Plugins);
            }
            catch (Exception e)
            {
                Log.Error($"Load Plugin [{di.Name}] Error: {e}");
            }
        }

    }

    public static void Init()
    {

    }
}
