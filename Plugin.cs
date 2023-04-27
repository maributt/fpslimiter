using Dalamud.Configuration;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading;
using ImGuiNET;

namespace FPSLimiter
{

    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }
        public int FpsCap = 60;
        public int FpsCapUnfocused = 20;
        public int FpsCapPrev = 20;
        public bool FpsCapEnabled = true;
        public bool FpsCapUnfocusedEnabled = true;
        public bool DisableOnLogin = false;
        public bool DisableOnZoning = true;

        [JsonIgnore] private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface PluginInterface)
        {
            pluginInterface = PluginInterface;
        }

        public void Save()
        {
            pluginInterface.SavePluginConfig(this);
        }
    }

    public class Plugin : IDalamudPlugin
    {
        public string Name => "fps limiter";
        public string Cmd => "/fps";

        private Stopwatch stopwatch;
        private Configuration settings;
        private DalamudPluginInterface pluginInterface;
        public bool Alternate = true;
        public bool ShowConfig = false;

        public Plugin(DalamudPluginInterface PluginInterface)
        {
            pluginInterface = PluginInterface;
            pluginInterface.Create<Svc>();
            settings = (Configuration)pluginInterface.GetPluginConfig() ?? new Configuration();
            stopwatch = new Stopwatch();

            Svc.Commands.AddHandler(Cmd, new CommandInfo(OnCmd)
            {
                HelpMessage = "sets your maximum fps - /fps # [bg|all]",
                ShowInHelp = true
            });
            Svc.Framework.Update += OnUpdate;
            pluginInterface.UiBuilder.Draw += Draw;
            pluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
        }
        
        public void ToggleConfig()
        {
            ShowConfig = !ShowConfig;
        }

        public void Draw()
        {
            if (!ShowConfig)
                return;
            ImGui.Begin("fps limiter config##configWindow", ref ShowConfig, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize);

            ImGui.Text("- fps caps");
            if (ImGui.Checkbox("##fpslimiterEnabled", ref settings.FpsCapEnabled))
            {
                pluginInterface.SavePluginConfig(settings);
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("enable/disable foreground fps cap");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(95);
            if (ImGui.InputInt("foreground##fpscap", ref settings.FpsCap, 1, 5))
            {
                if (settings.FpsCap < 5) settings.FpsCap = 5; // silly idea protection
                pluginInterface.SavePluginConfig(settings);
            }
            
            if (ImGui.Checkbox("##fpslimiterbgEnabled", ref settings.FpsCapUnfocusedEnabled))
            {
                pluginInterface.SavePluginConfig(settings);
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("enable/disable background fps cap");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(95);
            if (ImGui.InputInt("background##fpscapunfocused", ref settings.FpsCapUnfocused, 1, 1))
            {
                if (settings.FpsCapUnfocused < 1) settings.FpsCapUnfocused = 1;
                pluginInterface.SavePluginConfig(settings);
            }
            ImGui.Text("- disable plugin when...");
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextDisabled("allowing your game more resources during load times generally makes the transitions faster");
                ImGui.TextDisabled("you may want to use the \"logging in\" setting if a plugin requires a certain framerate to interact with the game on the character select screen");
                ImGui.TextDisabled("you can use the \"zoning\" setting to have a generally faster loading experience while still capping your frames while playing");
                ImGui.EndTooltip();
            }
            ImGui.Checkbox("...logging in##disableOnLogin", ref settings.DisableOnLogin);
            ImGui.Checkbox("...zoning##disableOnZoning", ref settings.DisableOnZoning);
            ImGui.End();
        }

        public void OnCmd(string command, string arg)
        {
            var args = arg.Split(' ');
            if (args.Length < 1) return;
            
            if (!int.TryParse(args[0], out int fpsCapNew))
            {
                ShowConfig = !ShowConfig;
                return;
            }
            if (args.Length == 2)
            {
                if (args[1].ToLower() == "bg")
                {
                    settings.FpsCapUnfocused = fpsCapNew;
                    Svc.Chat.PrintChat(new XivChatEntry()
                    {
                        Message = new SeString(new List<Payload>()
                        {
                            new TextPayload("Your background FPS is now capped to "),
                            new UIGlowPayload((ushort)551),
                            new TextPayload(settings.FpsCapUnfocused.ToString()),
                            new UIGlowPayload(0),
                            new TextPayload(".")
                        })
                    });
                }
                else if (args[1].ToLower() == "all")
                {
                    settings.FpsCapUnfocused = fpsCapNew;
                    settings.FpsCap = fpsCapNew;
                    Svc.Chat.PrintChat(new XivChatEntry()
                    {
                        Message = new SeString(new List<Payload>()
                        {
                            new TextPayload("Your background FPS and FPS are now capped to "),
                            new UIGlowPayload((ushort)541),
                            new TextPayload(settings.FpsCapUnfocused.ToString()),
                            new UIGlowPayload(0),
                            new TextPayload(".")
                        })
                    });
                }
                pluginInterface.SavePluginConfig(settings);
                return;
            }
            
            if (fpsCapNew == settings.FpsCap)
            {
                fpsCapNew = settings.FpsCapPrev;
                settings.FpsCapPrev = settings.FpsCap;
            }
            settings.FpsCap = fpsCapNew;

            Svc.Chat.PrintChat(new XivChatEntry()
            {
                Message = new SeString(new List<Payload>()
                {
                    new TextPayload("Your FPS is now capped to "),
                    new UIGlowPayload(Alternate ? (ushort)566 : (ushort)540),
                    new TextPayload(settings.FpsCap.ToString()),
                    new UIGlowPayload(0),
                    new TextPayload(".")
                })
            });

            Alternate = !Alternate;
            pluginInterface.SavePluginConfig(settings);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        public static bool IsGameFocused
        {
            get
            {
                var activatedHandle = GetForegroundWindow();
                if (activatedHandle == IntPtr.Zero)
                    return false;
                var procId = Environment.ProcessId;
                _ = GetWindowThreadProcessId(activatedHandle, out var activeProcId);
                return activeProcId == procId;
            }
        }

        public void OnUpdate(Framework framework)
        {
            if ((!settings.FpsCapEnabled && IsGameFocused) || (!settings.FpsCapUnfocusedEnabled && !IsGameFocused)) return;
            if (settings.DisableOnLogin && !Svc.ClientState.IsLoggedIn) return;
            if (settings.DisableOnZoning && Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas]) return;
            //var wantedMs = 1.0f / fpsCap * 1000;

            stopwatch.Stop();
            //var elapsedMS = stopwatch.ElapsedTicks / 10000f;
            //var sleepTime = Math.Max((1.0f / fpsCap * 1000) - (stopwatch.ElapsedTicks / 10000f), 0);

            Thread.Sleep((int)Math.Max((1.0f / (settings.FpsCapUnfocusedEnabled && !IsGameFocused ? settings.FpsCapUnfocused : settings.FpsCap ) * 1000) - (stopwatch.ElapsedTicks / 10000f), 0));
            stopwatch.Restart();
        }

        public void Dispose()
        {
            if (settings.FpsCap < 5) settings.FpsCap = 60;
            pluginInterface.SavePluginConfig(settings);
            Svc.Commands.RemoveHandler(Cmd);
            Svc.Framework.Update -= OnUpdate;
            pluginInterface.UiBuilder.Draw -= Draw;
            pluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
        }
    }
}
