using Dalamud.Configuration;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading;

namespace FPSLimiter
{

    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }
        public int FpsCap = 60;
        public int FpsCapUnfocused = 5;
        public int FpsCapPrev = 15;

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
        public string Name => "FPS Limiter";
        public string Cmd => "/fps";

        private Stopwatch stopwatch;
        private Configuration settings;
        private DalamudPluginInterface pluginInterface;
        private int fpsCap;
        private int fpsCapPrev;
        private int fpsCapUnfocused;
        public bool Alternate = true;

        public Plugin(DalamudPluginInterface PluginInterface)
        {
            pluginInterface = PluginInterface;
            pluginInterface.Create<Svc>();
            settings = (Configuration)pluginInterface.GetPluginConfig() ?? new Configuration();
            stopwatch = new Stopwatch();

            fpsCap = settings.FpsCap;
            fpsCapPrev = settings.FpsCapPrev;
            fpsCapUnfocused = settings.FpsCapUnfocused;

            Svc.Commands.AddHandler(Cmd, new CommandInfo(OnCmd)
            {
                HelpMessage = "Sets your maximum fps - /fps # [bg|all]",
                ShowInHelp = true
            });
            Svc.Framework.Update += OnUpdate;
        }

        public void OnCmd(string command, string arg)
        {
            var args = arg.Split(' ');
            if (args.Length < 1) return;
            if (!int.TryParse(args[0], out int fpsCapNew)) return;
            PluginLog.Log($"{args} : "+args.Length);
            if (args.Length == 2)
            {
                if (args[1].ToLower() == "bg")
                {
                    fpsCapUnfocused = fpsCapNew;
                    Svc.Chat.PrintChat(new XivChatEntry()
                    {
                        Message = new SeString(new List<Payload>()
                        {
                            new TextPayload("Your background FPS is now capped to "),
                            new UIGlowPayload((ushort)551),
                            new TextPayload(fpsCapUnfocused.ToString()),
                            new UIGlowPayload(0),
                            new TextPayload(".")
                        })
                    });
                }
                else if (args[1].ToLower() == "all")
                {
                    fpsCapUnfocused = fpsCapNew;
                    fpsCap = fpsCapNew;
                    Svc.Chat.PrintChat(new XivChatEntry()
                    {
                        Message = new SeString(new List<Payload>()
                        {
                            new TextPayload("Your background FPS and FPS are now capped to "),
                            new UIGlowPayload((ushort)541),
                            new TextPayload(fpsCapUnfocused.ToString()),
                            new UIGlowPayload(0),
                            new TextPayload(".")
                        })
                    });
                }
                pluginInterface.SavePluginConfig(settings);
                return;
            }
            
            //PluginLog.Log($"fpsCapNew: {fpsCapNew}, fpsCapPrev: ${fpsCapPrev}, fpsCap: {fpsCap}");
            if (fpsCapNew == fpsCap)
            {
                fpsCapNew = fpsCapPrev;
                fpsCapPrev = fpsCap;
            }
            fpsCap = fpsCapNew;

            Svc.Chat.PrintChat(new XivChatEntry()
            {
                Message = new SeString(new List<Payload>()
                {
                    new TextPayload("Your FPS is now capped to "),
                    new UIGlowPayload(Alternate ? (ushort)566 : (ushort)540),
                    new TextPayload(fpsCap.ToString()),
                    new UIGlowPayload(0),
                    new TextPayload(".")
                })
            });

            Alternate = !Alternate;
            settings.FpsCap = fpsCap;
            settings.FpsCapPrev = fpsCapPrev;

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
            //var wantedMs = 1.0f / fpsCap * 1000;
            stopwatch.Stop();
            //var elapsedMS = stopwatch.ElapsedTicks / 10000f;
            //var sleepTime = Math.Max((1.0f / fpsCap * 1000) - (stopwatch.ElapsedTicks / 10000f), 0);

            Thread.Sleep((int)Math.Max((1.0f / (IsGameFocused ? fpsCap : fpsCapUnfocused) * 1000) - (stopwatch.ElapsedTicks / 10000f), 0));
            stopwatch.Restart();
        }

        public void Dispose()
        {
            if (fpsCap < 5) settings.FpsCap = 60;
            pluginInterface.SavePluginConfig(settings);
            Svc.Commands.RemoveHandler(Cmd);
            Svc.Framework.Update -= OnUpdate;
        }
    }
}
