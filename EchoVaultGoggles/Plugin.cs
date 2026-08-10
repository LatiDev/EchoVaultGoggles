using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using EchoVaultGoggles.Displays;
using EchoVaultGoggles.Services;
using EchoVaultGoggles.Windows;

namespace EchoVaultGoggles;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static INamePlateGui NamePlateGui { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    private EchoVault EchoVault { get; init; }
    private TrackingDisplay TrackingDisplay { get; init; }

    public static Configuration Configuration { get; set; } = null!;

    public readonly WindowSystem WindowSystem = new("EchoVaultGoggles");
    private ConfigWindow ConfigWindow { get; init; }

    public Plugin()
    {
        // Must come first: TrackingDisplay subscribes to nameplate updates on construction,
        // and the handler reads Configuration.
        Plugin.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        EchoVault = new EchoVault();
        TrackingDisplay = new TrackingDisplay(EchoVault);

        ConfigWindow = new ConfigWindow();

        WindowSystem.AddWindow(ConfigWindow);

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleConfigUi;
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anything during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleConfigUi;
        
        WindowSystem.RemoveAllWindows();

        // Unsubscribe and cancel in-flight lookups before tearing down the client they use.
        TrackingDisplay.Dispose();
        EchoVault.Dispose();
        ConfigWindow.Dispose();
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
}
