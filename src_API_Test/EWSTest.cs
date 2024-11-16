using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using EntWatchSharpAPI;

namespace EWSTestAPI
{
	public class EWSTest : BasePlugin
	{
		public static IEntWatchSharpAPI? _EW_api;
		public override string ModuleName => "EntWatchSharp Test API";
		public override string ModuleDescription => "";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleVersion => "API.0.DZ.2";
		public override void OnAllPluginsLoaded(bool hotReload)
		{
			try
			{
				PluginCapability<IEntWatchSharpAPI> Capability = new("entwatch:api");
				_EW_api = IEntWatchSharpAPI.Capability.Get();
			}
			catch (Exception)
			{
				_EW_api = null;
				PrintToConsole("EntWatch API Failed!");
			}

			if (_EW_api != null)
			{
				_EW_api.Forward_EntWatch_OnClientBanned += DisplayBan;
				_EW_api.Forward_EntWatch_OnClientUnbanned += DisplayUnBan;
				_EW_api.Forward_EntWatch_OnUseItem += DisplayUseItem;
				_EW_api.Forward_EntWatch_OnPickUpItem += DisplayPickUpItem;
				_EW_api.Forward_EntWatch_OnDropItem += DisplayDropItem;
				_EW_api.Forward_EntWatch_OnPlayerDisconnectWithItem += DisplayDisconnectWithItem;
				_EW_api.Forward_EntWatch_OnPlayerDeathWithItem += DisplayDeathWithItem;
				_EW_api.Forward_EntWatch_OnAdminSpawnItem += DisplayAdminSpawnItem;
				_EW_api.Forward_EntWatch_OnAdminTransferedItem += DisplayAdminTransferedItem;
			}
		}

		void DisplayBan(SEWAPI_Ban sewPlayer) { PrintToConsole($"Player {sewPlayer.sClientName} was banned {sewPlayer.sAdminName}"); }
		void DisplayUnBan(SEWAPI_Ban sewPlayer) { PrintToConsole($"Player {sewPlayer.sClientName} was unbanned {sewPlayer.sAdminName}"); }
		void DisplayUseItem(string sItemName, CCSPlayerController Player, string sAbility) { PrintToConsole($"Player {Player.PlayerName} used {sItemName}({sAbility})"); }
		void DisplayPickUpItem(string sItemName, CCSPlayerController Player) { PrintToConsole($"Player {Player.PlayerName} pickup {sItemName}"); }
		void DisplayDropItem(string sItemName, CCSPlayerController Player) { PrintToConsole($"Player {Player.PlayerName} dropped {sItemName}"); }
		void DisplayDisconnectWithItem(string sItemName, CCSPlayerController Player) { PrintToConsole($"Player {Player.PlayerName} disconnected with {sItemName}"); }
		void DisplayDeathWithItem(string sItemName, CCSPlayerController Player) { PrintToConsole($"Player {Player.PlayerName} death with {sItemName}"); }
		void DisplayAdminSpawnItem(CCSPlayerController Admin, string sItemName, CCSPlayerController Target) { PrintToConsole($"Admin {Admin.PlayerName} spawned {sItemName} for {Target.PlayerName}"); }
		void DisplayAdminTransferedItem(CCSPlayerController Admin, string sItemName, CCSPlayerController Receiver) { PrintToConsole($"Admin {Admin.PlayerName} transfered {sItemName} for {Receiver.PlayerName}"); }

		[ConsoleCommand("ewt_1", "")]
		[RequiresPermissions("@css/ew_ban")]
		public async void OnEWT1(CCSPlayerController? player, CommandInfo command)
		{
			if (_EW_api == null || player == null || !player.IsValid) return;
			SEWAPI_Ban ban = await _EW_api.Native_EntWatch_IsClientBanned(ConvertSteamID64ToSteamID(player.SteamID.ToString()));
			if (ban.bBanned) PrintToConsole($"You {ban.sClientName}({ban.sClientSteamID}) have a eban. Duration: {ban.iDuration}");
			else PrintToConsole($"You have NOT a eban");
		}

		[ConsoleCommand("ewt_2", "")]
		[RequiresPermissions("@css/ew_ban")]
		public async void OnEWT2(CCSPlayerController? player, CommandInfo command)
		{
			if (_EW_api == null || player == null || !player.IsValid) return;
			SEWAPI_Ban ban = new SEWAPI_Ban();
			ban.sAdminName = "Api";
			ban.sAdminSteamID = "SERVER";
			ban.iDuration = 5;
			ban.iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()) + ban.iDuration * 60;
			ban.sReason = "Test Api Ban";
			ban.sClientName = player.PlayerName;
			ban.sClientSteamID = ConvertSteamID64ToSteamID(player.SteamID.ToString());
			if (await _EW_api.Native_EntWatch_BanClient(ban)) PrintToConsole("Successfully banned");
			else PrintToConsole("Failed to ban");
		}

		[ConsoleCommand("ewt_3", "")]
		[RequiresPermissions("@css/ew_unban")]
		public async void OnEWT3(CCSPlayerController? player, CommandInfo command)
		{
			if (_EW_api == null || player == null || !player.IsValid) return;
			SEWAPI_Ban ban = new SEWAPI_Ban();
			ban.sAdminName = "Api";
			ban.sAdminSteamID = "SERVER";
			ban.iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()) + ban.iDuration * 60;
			ban.sReason = "Test Api UnBan";
			ban.sClientName = player.PlayerName;
			ban.sClientSteamID = ConvertSteamID64ToSteamID(player.SteamID.ToString());
			if (await _EW_api.Native_EntWatch_UnbanClient(ban)) PrintToConsole("Successfully unbanned");
			else PrintToConsole("Failed to unban");
		}

		[ConsoleCommand("ewt_4", "")]
		[RequiresPermissions("@css/ew_unban")]
		public async void OnEWT4(CCSPlayerController? player, CommandInfo command)
		{
			if (_EW_api == null || player == null || !player.IsValid) return;
			await _EW_api.Native_EntWatch_UpdateStatusBanClient(player);
		}

		[ConsoleCommand("ewt_5", "")]
		[RequiresPermissions("@css/ew_reload")]
		public void OnEWT5(CCSPlayerController? player, CommandInfo command)
		{
			if (_EW_api == null || player != null && !player.IsValid) return;
			try
			{
				int number;
				if (!Int32.TryParse(command.GetArg(1), out number)) number = 0;
				CEntityInstance? cEntity = Utilities.GetEntityFromIndex<CEntityInstance>(number);
				if (cEntity != null && cEntity.IsValid)
				{
					if (_EW_api.Native_EntWatch_IsSpecialItem(cEntity)) PrintToConsole("Entity is Special Item");
					else PrintToConsole("Entity is NOT Special Item");
				}
				else PrintToConsole("Error on number of entity");
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		[ConsoleCommand("ewt_6", "")]
		[RequiresPermissions("@css/ew_reload")]
		public void OnEWT6(CCSPlayerController? player, CommandInfo command)
		{
			if (_EW_api == null || player != null && !player.IsValid) return;
			try
			{
				int number;
				if (!Int32.TryParse(command.GetArg(1), out number)) number = 0;
				CEntityInstance? cEntity = Utilities.GetEntityFromIndex<CEntityInstance>(number);
				if (cEntity != null && cEntity.IsValid)
				{
					if (_EW_api.Native_EntWatch_IsButtonSpecialItem(cEntity)) PrintToConsole("Entity is a Special Item Button");
					else PrintToConsole("Entity is NOT a Special Item Button");
				}
				else PrintToConsole("Error on number of entity");
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		[ConsoleCommand("ewt_7", "")]
		[RequiresPermissions("@css/ew_reload")]
		public void OnEWT7(CCSPlayerController? player, CommandInfo command)
		{
			if (_EW_api == null || player == null || !player.IsValid) return;
			if (_EW_api.Native_EntWatch_HasSpecialItem(player)) PrintToConsole("You have a Special Item");
			else PrintToConsole("You have NOT a Special Item");
		}

		public static void PrintToConsole(string sMessage)
		{
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("[");
			Console.ForegroundColor = (ConsoleColor)6;
			Console.Write("EntWatch:TestAPI");
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("] ");
			Console.ForegroundColor = (ConsoleColor)13;
			Console.WriteLine(sMessage);
			Console.ResetColor();
		}
		public static string ConvertSteamID64ToSteamID(string steamId64)
		{
			if (ulong.TryParse(steamId64, out var communityId) && communityId > 76561197960265728)
			{
				var authServer = (communityId - 76561197960265728) % 2;
				var authId = (communityId - 76561197960265728 - authServer) / 2;
				return $"STEAM_1:{authServer}:{authId}";
			}
			return null;
		}
	}
}
