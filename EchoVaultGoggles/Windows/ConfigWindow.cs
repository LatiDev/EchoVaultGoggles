using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace EchoVaultGoggles.Windows;

public class ConfigWindow : Window, IDisposable
{
    public ConfigWindow() : base("Configuration Window")
    {
        this.Flags |= ImGuiWindowFlags.NoResize;
        this.Flags |= ImGuiWindowFlags.NoCollapse;
        this.Flags |= ImGuiWindowFlags.NoScrollbar;
        this.Flags |= ImGuiWindowFlags.NoScrollWithMouse;
        this.Flags &= ~ImGuiWindowFlags.NoMove;

        this.Size = new Vector2(232, 90);
        this.SizeCondition = ImGuiCond.Always;
    }

    public override void Draw()
    {
        var apiKey = Plugin.Configuration.apiKey;

        ImGui.TextUnformatted("API Key");
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputText("##apiKey", ref apiKey, 256, ImGuiInputTextFlags.Password))
        {
            Plugin.Configuration.apiKey = apiKey;
            Plugin.Configuration.Save();
        }
    }

    public void Dispose()
    {
        
    }
}
