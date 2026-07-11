using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace SamplePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    public int CooldownSeconds { get; set; } = 30;
    public bool SendToParty { get; set; } = true;
    public bool IsConfigWindowMovable { get; set; } = false;
    public bool DebugMode { get; set; } = false;

    // We give this window a constant ID using ###.
    // This allows for labels to be dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("A Wonderful Configuration Window###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(500, 500);
        SizeCondition = ImGuiCond.Always;

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        var changed = false;
        var cooldown = configuration.CooldownSeconds;
        if (ImGui.SliderInt("Cooldown (seconds)", ref cooldown, 5, 300))
        {
            configuration.CooldownSeconds = cooldown;
            changed = true;
        }
        if (changed)
        {
            configuration.Save();
        }

        var sendToParty = configuration.SendToParty;
        if (ImGui.Checkbox("Announce in Party Chat", ref sendToParty))
        {
            if (configuration.debugMode)
            {
                configuration.debugMode = false;
            }
            configuration.SendToParty = sendToParty;
            configuration.Save();
        }

        var debugMode = configuration.debugMode;
        if (ImGui.Checkbox("Debug Mode (No need for Party)", ref debugMode))
        {
            configuration.debugMode = debugMode;
            configuration.SendToParty = false;
            configuration.Save();
        }

        ImGui.Separator();

        var movable = configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            configuration.IsConfigWindowMovable = movable;
            configuration.Save();
        }
    }
}
