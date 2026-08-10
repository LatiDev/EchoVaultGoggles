using Dalamud.Configuration;
using System;

namespace SamplePlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public string apiKey { get; set; } = String.Empty;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
