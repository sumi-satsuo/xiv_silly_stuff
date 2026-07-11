using Dalamud.Game.Chat;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.DalamudServices.Legacy;
using SamplePlugin.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IChatGui DalaChat { get; private set; } = null!;
    [PluginService] internal static IPartyList PartyList { get; private set; } = null!; 
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string TruthOrDareCommand = "/tod";
    

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private TruthOrDare TruthOrDare { get; init; }

    
    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // You might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ECommonsMain.Init(PluginInterface, this);
        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);
        TruthOrDare = new TruthOrDare(PartyList);
        //TODO Make a russianRoulette game xD

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);


    CommandManager.AddHandler(TruthOrDareCommand, new CommandInfo(OnTruthOrDareCommand)
        {
            HelpMessage = "Play a game of Truth or Dare with your party members."
        });

        // Tell the UI system that we want our windows to be drawn through the window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anything during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        ECommonsMain.Dispose();

        CommandManager.RemoveHandler(TruthOrDareCommand);
    }

    private void OnTruthOrDareCommand(string command, string args) // command: >tod< play
    {
        var splitArgs = args.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var subcommand = splitArgs.FirstOrDefault()?.ToLower() ?? ""; //subcommand: tod >play<
        var subcommand_optAmount = 1;

        switch (subcommand)
        {
            case "play":
                var result = TruthOrDare.Play(Configuration);
                if (result.Success)
                {
                    var (winner, loser) = result.Value;
                    var resMessage = $"/p  Asker: {winner} | Victim: {loser} ";
                    if (Configuration.debugMode)
                    {
                        resMessage += " (Debug Mode)";
                        DalaChat.Print($"/p  Truth or Dare!! Round {TruthOrDare.CurrentRound} ");
                        DalaChat.Print(resMessage);
                    }
                    else
                    {
                        Chat.SendMessage($"/p  Truth or Dare!! Round {TruthOrDare.CurrentRound} ");
                        Chat.SendMessage(resMessage);
                    }
                }
                else
                {
                    DalaChat.PrintError($"{result.Error}");
                }
                
                break;

            case "last":
                if (splitArgs.Length > 1)
                {
                    if (!int.TryParse(splitArgs[1], out subcommand_optAmount) || subcommand_optAmount <= 0)
                    {
                        DalaChat.PrintError("Invalid amount specified. Please provide a valid number.");
                        return;
                    }
                }

                if (subcommand_optAmount > 1)
                {
                    var lastMatches = TruthOrDare.GetLast(subcommand_optAmount);
                    Chat.SendMessage($"/p  Last {subcommand_optAmount} round results: ");
                    foreach (var match in lastMatches)
                    {
                        Chat.SendMessage($" Asker: {match.Winner} | Victim: {match.Loser} | Round: {match.CurrentRound} ");
                        //small delay to avoid flooding the chat
                    }
                    return;
                }
                else
                {
                    var lastMatch = TruthOrDare.GetLast(1).FirstOrDefault();
                    if (lastMatch.Winner == null || lastMatch.Loser == null)
                    {
                        DalaChat.Print("No matches have been played yet.");
                        return;
                    }
                    Chat.SendMessage($"/p  Last round results: ");
                    Chat.SendMessage($" Asker: {lastMatch.Winner} | Victim: {lastMatch.Loser} | Round: {lastMatch.CurrentRound} ");
                }
                break;

            case "reset":
                TruthOrDare.Reset();
                DalaChat.Print("Rounds have been reset.");
                break;

            case "config":
                ToggleConfigUi();
                break;

            default:
                DalaChat.Print("Play using: /tod play");
                DalaChat.Print("Check last match: /tod last");
                DalaChat.Print("Reset Rounds: /tod reset");
                break;
        }   
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}
