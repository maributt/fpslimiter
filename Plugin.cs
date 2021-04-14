using Dalamud.Plugin;
using FPSLimiter.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace FPSLimiter
{
    public class Plugin : IDalamudPlugin
    {
        private PluginCommandManager<Plugin> commandManager;
        private Stopwatch Timer;
        private Dalamud.Game.Internal.Gui.ChatGui Chat;
        private DalamudPluginInterface Interface;
        private Configuration Config;

        private int CurrentFpsCap;
        private int PreviousFpsCap;
        private IntPtr FpsDividerPtr;
        private ushort[] ChatCol = new ushort[] { 566, 540 };

        private bool Initialized = false;

        private static readonly string IgDividerSig = "48 89 05 ?? ?? ?? ?? EB 07 48 89 3D ?? ?? ?? ?? BA";

        public string Name => "FPSLimiter";

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.Interface = pluginInterface;
            this.Chat = Interface.Framework.Gui.Chat;
            this.Timer = new Stopwatch();

            this.Config = (Configuration)Interface.GetPluginConfig() ?? new Configuration();
            this.Config.Initialize(Interface);

            this.CurrentFpsCap = Config.CurrentFpsCap;
            this.PreviousFpsCap = Config.PreviousFpsCap;

            var gKernalDevice = this.Interface.TargetModuleScanner.GetStaticAddressFromSig(IgDividerSig);
            this.FpsDividerPtr = Marshal.ReadIntPtr(gKernalDevice) + 0x13C;

            Interface.Framework.OnUpdateEvent += Update;
            this.commandManager = new PluginCommandManager<Plugin>(this, Interface);

            this.Initialized = true;
        }
        public void Update(Dalamud.Game.Internal.Framework framework)
        {
            if (!Initialized || this.CurrentFpsCap <= 4) return;
            
            var wantedMS = 1.0f / this.CurrentFpsCap * 1000;
            Timer.Stop();
            var elapsedMS = Timer.ElapsedTicks / 10000f;
            var sleepTime = Math.Max(wantedMS - elapsedMS, 0);
            Thread.Sleep((int)sleepTime);
            Timer.Restart();
        }

        public void DisableCustomCap()
        {
            this.CurrentFpsCap = -1;
            return;
        }

        public void UpdateCurrentFpsCap(int NewFpsCap)
        {
            UpdateCurrentFpsDivider(0);
            this.CurrentFpsCap = NewFpsCap;
            PrintFps("Your FPS cap is now set to ", ""+NewFpsCap, ChatCol[0]);
        }

        public void UpdateCurrentFpsDivider(int NewFpsDivider, bool silent=true)
        {
            Marshal.WriteByte(this.FpsDividerPtr, (byte)NewFpsDivider);
            if (!silent)
            {
                DisableCustomCap();
                this.CurrentFpsCap = NewFpsDivider;
                var FpsString = NewFpsDivider == 0 ? "None" : $"Refresh Rate 1/{NewFpsDivider}";
                PrintFps("Your FPS cap is now set to ", FpsString, ChatCol[1]);
            }
        }

        public void PrintFps(string msg, string fps, ushort col=570)
        {
            var message = new List<Payload>()
                {
                    new TextPayload(msg),
                    new UIGlowPayload(this.Interface.Data, col),
                    new TextPayload(fps),
                    new UIGlowPayload(this.Interface.Data, 0),
                    new TextPayload(".")
                };
            Chat.PrintChat(new XivChatEntry
            {
                MessageBytes = new SeString(message).Encode()
            });
        }

        [Command("/fps")]
        [HelpMessage("" +
                "Usage:   /fps #\n" +
                " FPS Limits:\n" +
                "   0:      None\n" +
                "   1..4:   Refresh Rate 1/1 ... 1/4\n" +
                "   5..999: Custom FPS caps\n" +
                " Tip:\n" +
                "   Using \"/fps #\" two times in a row will toggle\n" +
                "   between your current and previous FPS cap!")]
        public void FpsCommand(string command, string arguments)
        {
            var args = arguments.Split(' ');
            switch (args.Length)
            {
                case 0:
                    SyntaxHelp();
                    break;
                case 1:
                    if (int.TryParse(args[0], out int NewFpsCap))
                    {
                        if (NewFpsCap < 0) return;

                        if (NewFpsCap == this.CurrentFpsCap) NewFpsCap = PreviousFpsCap;
                        PreviousFpsCap = this.CurrentFpsCap;

                        if (NewFpsCap >= 5)
                        {
                            UpdateCurrentFpsCap(NewFpsCap);
                        } 
                        else
                        {
                            UpdateCurrentFpsDivider(NewFpsCap, false);
                        }
                    } else SyntaxHelp();
                    break;
                default:
                    SyntaxHelp();
                    break;
            }
        }

        private void SyntaxHelp()
        {
            Chat.PrintError(
                "" +
                "Usage:   /fps #\n" +
                " FPS Limits:\n" +
                "   0:      None\n" +
                "   1..4:   Refresh Rate 1/1 ... 1/4\n" +
                "   5..999: Custom FPS caps\n" +
                " Tip:\n" +
                "   Using \"/fps #\" two times in a row will toggle\n" +
                "   between your current and previous FPS cap!"
            );
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            Config.CurrentFpsCap = this.CurrentFpsCap;
            Config.PreviousFpsCap = this.PreviousFpsCap;
            this.commandManager.Dispose();
            Interface.Framework.OnUpdateEvent -= Update;
            this.Interface.SavePluginConfig(this.Config);
            this.Interface.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
