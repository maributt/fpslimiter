using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace FPSLimiter
{
	class Svc
	{
		[PluginService] static internal DalamudPluginInterface PluginInterface { get; private set; }
		[PluginService] static internal ChatGui Chat { get; private set; }
		[PluginService] static internal CommandManager Commands { get; private set; }
		[PluginService] static internal Framework Framework { get; private set; }
#pragma warning disable CS0618 // Type or member is obsolete
        [PluginService] static internal SeStringManager SeStringManager { get; private set; }
#pragma warning restore CS0618 // Type or member is obsolete
	}
}