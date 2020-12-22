using Dalamud.Game.Command;
using Dalamud.Plugin;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FPSLimiter
{
    public class Plugin: IDalamudPlugin
    {
        public string Name => "FPS Limiter";

        private const string command = "/fps";

        
        private DalamudPluginInterface pi;
        private FPSControl fc;


        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pi = pluginInterface;
            this.fc = new FPSControl(this.pi);


            this.pi.CommandManager.AddHandler(command, new CommandInfo(OnCommand)
            {
                HelpMessage = "Manage your fps cap with a command and from within macros!\nExample: /fps 2  -> Refresh Rate 1/2"
            });
        }

        private void OnCommand(string command, string args)
        {

            string[] argument_list = args.Split(' ');

            if (argument_list.Length == 0)
            {
                helpMessage();
                return;
            }
            
            string arg = argument_list[0];
            int argAsNumber = this.processArg(arg);

            if (argAsNumber == -1)
            {
                helpMessage();
                return;
            }

            fc.writeFps(argAsNumber);
            this.pi.Framework.Gui.Chat.Print($"Your FPS cap has been set to: {fc.GetFpsDividerName()}");
            
        }

        private int processArg(string arg)
        {
            int argAsNumber;
            bool isNumber = int.TryParse(arg, out argAsNumber);
            switch (arg.ToLower())
            {
                case "1/4":
                case "2/4":
                case "3/4":
                case "4/4":
                    int.TryParse(arg.Split('/')[0], out argAsNumber);
                    break;
                case "previous":
                    argAsNumber = fc.getCurrentFpsCap();
                    break;
            }
            return argAsNumber;

        }

        private void helpMessage()
        {
            this.pi.Framework.Gui.Chat.Print($"Usage: /fps <0..4>\nFPS Limits: (You can also use 1/1..1/4 instead of numbers!)\n   0: None.\n   1: Refresh Rate 1/1\n   2: Refresh Rate 1/2\n   3: Refresh Rate 1/3\n   4: Refresh Rate 1/4\nUsing \"/fps <n>\" two times in a row or \"/fps previous\" will take you back to\nthe fps cap you had before using it the first time. ");
        }

        public void Dispose()
        {
            this.pi.CommandManager.RemoveHandler(command);
            this.pi.Dispose();
        }

        
    }
}
