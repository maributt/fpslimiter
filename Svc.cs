using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace FPSLimiter
{
	class Svc
	{
		[PluginService] static internal IDalamudPluginInterface PluginInterface { get; private set; }
		[PluginService] static internal IChatGui Chat { get; private set; }
		[PluginService] static internal ICommandManager Commands { get; private set; }
        [PluginService] static internal IClientState ClientState { get; private set; }
        [PluginService] static internal IFramework Framework { get; private set; }
		[PluginService] static internal ICondition Condition { get; private set; }
	}
}