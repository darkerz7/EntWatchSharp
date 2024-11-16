using ClientPrefsAPI;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Timers;
using EntWatchSharp.Helpers;
using EntWatchSharp.Modules.Eban;
using EntWatchSharpAPI;
using Microsoft.Extensions.Localization;

namespace EntWatchSharp
{
	[MinimumApiVersion(285)]
	public partial class EntWatchSharp : BasePlugin
	{
		public static IStringLocalizer Strlocalizer;
		public override string ModuleName => "EntWatchSharp";
		public override string ModuleDescription => "Notify players about entity interactions";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleVersion => "0.DZ.9.beta";

		public override void OnAllPluginsLoaded(bool hotReload)
		{
			try
			{
				PluginCapability<IClientPrefsAPI> CapabilityCP = new("clientprefs:api");
				EW._CP_api = IClientPrefsAPI.Capability.Get();
			}
			catch (Exception)
			{
				EW._CP_api = null;
				UI.EWSysInfo("Info.Error", 15, "ClientPrefs API Failed!");
			}

			try
			{
				PluginCapability<IEntWatchSharpAPI> CapabilityEW = new("entwatch:api");
				EW._EW_api = IEntWatchSharpAPI.Capability.Get();
			}
			catch (Exception)
			{
				EW._EW_api = null;
				UI.EWSysInfo("Info.Error", 15, "EntWatch API Failed!");
			}

			if (hotReload)
			{
				Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(player =>
				{
					EW.LoadClientPrefs(player);
				});
			}
		}

		public override void Load(bool hotReload)
		{
			Strlocalizer = Localizer;

			RegisterCVARS();

			try
			{
				EW.g_cAPI = new EWAPI();
				Capabilities.RegisterPluginCapability(IEntWatchSharpAPI.Capability, () => EW.g_cAPI);
			}
			catch (Exception)
			{
				EW.g_cAPI = null;
				UI.EWSysInfo("Info.Error", 15, "EntWatch API Register Failed!");
			}

			if (hotReload)
			{
				EW.LoadScheme();
				EW.LoadConfig();
				EW.g_Timer = new CounterStrikeSharp.API.Modules.Timers.Timer(1.0f, TimerUpdate, TimerFlags.REPEAT);
				Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(player =>
				{
					EW.CheckDictionary(player, EW.g_HudPlayer);

					EW.CheckDictionary(player, EW.g_UsePriorityPlayer);

					//EW.LoadClientPrefs(player);

					EW.CheckDictionary(player, EW.g_BannedPlayer);

					OfflineFunc.PlayerConnectFull(player);
				});
				EW.g_TimerRetryDB = new CounterStrikeSharp.API.Modules.Timers.Timer(1.0f, TimerRetry, TimerFlags.REPEAT);
			}
			EW.g_TimerUnban = new CounterStrikeSharp.API.Modules.Timers.Timer(60.0f, TimerUnban, TimerFlags.REPEAT);

			RegEvents();
			VirtualFunctionsInitialize();
			EbanDB.Init_DB(ModuleDirectory);
			LogManager.LoadConfig(ModuleDirectory);
			UI.EWSysInfo("Info.EWLoaded", 6);
		}

		public override void Unload(bool hotReload)
		{
			EW.CleanData();
			VirtualFunctionsUninitialize();
			UnRegEvents();
			UnRegCommands();
			UnRegMapCommands();
			if (EW.g_Timer != null)
			{
				EW.g_Timer.Kill();
				EW.g_Timer = null;
			}
			if (EW.g_TimerRetryDB != null)
			{
				EW.g_TimerRetryDB.Kill();
				EW.g_TimerRetryDB = null;
			}
			if (EW.g_TimerUnban != null)
			{
				EW.g_TimerUnban.Kill();
				EW.g_TimerUnban = null;
			}
			LogManager.UnInit();
			Utilities.GetPlayers().ForEach(player =>
			{
				if (player.IsValid)
				{
					if (EW.CheckDictionary(player, EW.g_HudPlayer))
					{
						EW.RemoveEntityHud(player);
					}
				}
			});
		}
	}
}
