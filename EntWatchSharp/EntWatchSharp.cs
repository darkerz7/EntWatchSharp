using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Timers;
using CS2_GameHUDAPI;
using EntWatchSharp.Helpers;
using EntWatchSharp.Modules.Eban;
using EntWatchSharpAPI;
using Microsoft.Extensions.Localization;

namespace EntWatchSharp
{
	[MinimumApiVersion(330)]
	public partial class EntWatchSharp : BasePlugin
	{
		public static IStringLocalizer Strlocalizer;
		public override string ModuleName => "EntWatchSharp";
		public override string ModuleDescription => "Notify players about entity interactions";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleVersion => "1.DZ.6.2";

		public override void OnAllPluginsLoaded(bool hotReload)
		{
			EW._PlayerSettingsAPI = EW._PlayerSettingsAPICapability.Get();
			if (EW._PlayerSettingsAPI == null)
				UI.EWSysInfo("Info.Error", 15, "PlayerSettings API Failed!");

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

			try
			{
				PluginCapability<IGameHUDAPI> CapabilityCP = new("gamehud:api");
				EW._GH_api = IGameHUDAPI.Capability.Get();
			}
			catch (Exception)
			{
				EW._GH_api = null;
				UI.EWSysInfo("Info.Error", 15, "GameHUD API Failed!");
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
					EW.CheckDictionary(player);

					//EW.LoadClientPrefs(player);

					OfflineFunc.PlayerConnectFull(player);
				});
				EW.g_TimerRetryDB = new CounterStrikeSharp.API.Modules.Timers.Timer(1.0f, TimerRetry, TimerFlags.REPEAT);
			}
			EW.g_TimerUnban = new CounterStrikeSharp.API.Modules.Timers.Timer(60.0f, TimerUnban, TimerFlags.REPEAT);

			RegEvents();
			RegisterListener<Listeners.OnMetamodAllPluginsLoaded>(() =>
			{
				VirtualFunctionsInitialize();
			});
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
					if (EW.CheckDictionary(player))
					{
						EW.g_EWPlayer[player].RemoveEntityHud(player);
					}
				}
			});
		}
	}
}
