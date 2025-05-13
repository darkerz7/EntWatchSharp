using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json;
using EntWatchSharp.Items;
using EntWatchSharp.Helpers;
using EntWatchSharp.Modules.Eban;
using EntWatchSharpAPI;
using CS2_GameHUDAPI;
using System.Globalization;
using CounterStrikeSharp.API.Core.Capabilities;
using PlayerSettings;

namespace EntWatchSharp
{
    static class EW
	{
		public static readonly byte HUDCHANNEL = 10;

		public static double fGameTime;
		public static List<ItemConfig> g_ItemConfig = [];
		public static List<Item> g_ItemList = [];
		public static Scheme g_Scheme = new();
		public static bool g_CfgLoaded = false;
		public static CultureInfo cultureEN = new("en-EN");

#nullable enable
		public static ISettingsApi? _PlayerSettingsAPI;
		public readonly static PluginCapability<ISettingsApi?> _PlayerSettingsAPICapability = new("settings:nfcore");
#nullable disable

		public static IEntWatchSharpAPI _EW_api;
		public static EWAPI g_cAPI = null;

		public static IGameHUDAPI _GH_api;

		public static Dictionary<CCSPlayerController, EWPlayer> g_EWPlayer = [];
		public static List<OfflineBan> g_OfflinePlayer = [];

		public static CounterStrikeSharp.API.Modules.Timers.Timer g_Timer = null;
		public static CounterStrikeSharp.API.Modules.Timers.Timer g_TimerRetryDB = null;
		public static CounterStrikeSharp.API.Modules.Timers.Timer g_TimerUnban = null;

		public static CounterStrikeSharp.API.Modules.Utils.Vector vecNull = new(0, 0, 0);

		public static bool CheckDictionary(CCSPlayerController player)
		{
			if (!g_EWPlayer.ContainsKey(player))
				return g_EWPlayer.TryAdd(player, new EWPlayer());
			return true;
		}

		public static void UpdateTime()
		{
			fGameTime = Server.EngineTime;
		}
		public static void CleanData()
		{
			g_CfgLoaded = false;
			g_ItemList.Clear();
			g_ItemConfig.Clear();
		}
		public static void LoadConfig()
		{
			string sFileName = $"../../csgo/{Cvar.PathCfg}{(Cvar.LowerMapname ? Server.MapName.ToLower() : Server.MapName)}.json";
			string sFileNameOverride = $"../../csgo/{Cvar.PathCfg}{(Cvar.LowerMapname ? Server.MapName.ToLower() : Server.MapName)}_override.json";
			string sData;
			if (File.Exists(sFileNameOverride))
			{
				sData = File.ReadAllText(sFileNameOverride);
				UI.EWSysInfo("Info.Cfg.Loading", 7, sFileNameOverride);
			}
			else if (File.Exists(sFileName))
			{
				sData = File.ReadAllText(sFileName);
				UI.EWSysInfo("Info.Cfg.Loading", 7, sFileName);
			}
			else
			{
				UI.EWSysInfo("Info.Cfg.NotFound", 14);
				return;
			}
			g_ItemConfig = JsonSerializer.Deserialize<List<ItemConfig>>(sData);
			g_CfgLoaded = true;
		}

		public static void LoadScheme()
		{
			string sFileName = $"../../csgo/{Cvar.PathScheme}";
			string sData;
			if (File.Exists(sFileName) || File.Exists(sFileName = "../../csgo/addons/entwatch/scheme/default.json"))
			{
				sData = File.ReadAllText(sFileName);
				UI.EWSysInfo("Info.Scheme.Loading", 7, sFileName);
			}
			else
			{
				UI.EWSysInfo("Info.Scheme.NotFound", 14);
				return;
			}
			g_Scheme = JsonSerializer.Deserialize<Scheme>(sData);
		}

		public static bool WeaponIsItem(CEntityInstance entity)
		{
			var weapon = new CCSWeaponBase(entity.Handle);
			if (weapon == null || !weapon.IsValid) return false;
			try
			{
				int iHammerID = Int32.Parse(weapon.UniqueHammerID);
				foreach (ItemConfig ItemTest in g_ItemConfig.ToList())
				{
					if (ItemTest.ThisItemConfig(iHammerID))
					{
						Item cNewItem = new(ItemTest, weapon);
						g_ItemList.Add(cNewItem);
						return true;
					}
				}
			}
			catch (Exception){	}
			return false;
		}

#nullable enable
		public static CEntityInstance? EntityParentRecursive(CEntityInstance entity)
#nullable disable
		{
			if (entity == null || !entity.IsValid) return null;

			if (entity.DesignerName.Contains("weapon_")) return entity;

			var baseentity = new CBaseEntity(entity.Handle);
			if (baseentity == null || !baseentity.IsValid) return null;
#nullable enable
			CEntityInstance? Owner = baseentity.CBodyComponent?.SceneNode?.PParent?.Owner;
#nullable disable
			if (Owner == null || !Owner.IsValid) return null;
			if (Owner.Entity != null && Owner.Entity.Name != null && !string.Equals(Owner.Entity.Name, ""))
			{
				var ownerentity = baseentity.CBodyComponent?.SceneNode?.PParent?.Owner;
				if (ownerentity == null || !ownerentity.IsValid) return null;
				return EntityParentRecursive(ownerentity);
			}
			return null;
		}

		public static void LoadClientPrefs(CCSPlayerController player)
		{
			try
			{
				string sHUDType = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_HUD_Type", "3") : "3";
				string sHUDPos = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_HUD_Pos", "-6.5_2_7") : "-6.5_2_7";
				string sHUDSize = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_HUD_Size", "54") : "54";
				string sHUDColor = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_HUD_Color", "255_255_255_255") : "255_255_255_255";
				string sHUDRefresh = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_HUD_Refresh", "3") : "3";
				string sHUDSheet = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_HUD_Sheet", "5") : "5";
				string sPlayerInfoFormat = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_PInfo_Format", $"{Cvar.PlayerFormat}") : $"{Cvar.PlayerFormat}";

				string sUsePriority = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_Use_Priority", "1") : "1";
				if (player.IsValid && CheckDictionary(player))
				{
					if (!string.IsNullOrEmpty(sHUDPos))
					{
						try
						{
							string[] Pos = sHUDPos.Replace(',','.').Split(['_']);
							if (Pos[0] != null && Pos[1] != null && Pos[2] != null)
							{
								g_EWPlayer[player].HudPlayer.fXEntity = float.Parse(Pos[0], NumberStyles.Any, cultureEN);
								g_EWPlayer[player].HudPlayer.fYEntity = float.Parse(Pos[1], NumberStyles.Any, cultureEN);
								g_EWPlayer[player].HudPlayer.fZEntity = float.Parse(Pos[2], NumberStyles.Any, cultureEN);
							}
						}
						catch { }
					}
					if (!string.IsNullOrEmpty(sHUDColor))
					{
						try
						{
							string[] Pos = sHUDColor.Split(['_']);
							if (Pos[0] != null && Pos[1] != null && Pos[2] != null && Pos[3] != null)
							{
								g_EWPlayer[player].HudPlayer.colorEntity[0] = Int32.Parse(Pos[0]);
								g_EWPlayer[player].HudPlayer.colorEntity[1] = Int32.Parse(Pos[1]);
								g_EWPlayer[player].HudPlayer.colorEntity[2] = Int32.Parse(Pos[2]);
								g_EWPlayer[player].HudPlayer.colorEntity[3] = Int32.Parse(Pos[3]);
							}
						}
						catch { }
					}
					if (!string.IsNullOrEmpty(sHUDSize))
					{
						if (!Int32.TryParse(sHUDSize, out int number)) number = 54;
						g_EWPlayer[player].HudPlayer.iSize = number;
					}
					if (!string.IsNullOrEmpty(sHUDType))
					{
						if (!Int32.TryParse(sHUDType, out int number)) number = 3;
						g_EWPlayer[player].SwitchHud(player, number);
					}
					if (!string.IsNullOrEmpty(sHUDRefresh))
					{
						if (Int32.TryParse(sHUDRefresh, out int number)) g_EWPlayer[player].HudPlayer.iRefresh = number;
					}
					if (!string.IsNullOrEmpty(sHUDSheet))
					{
						if (Int32.TryParse(sHUDSheet, out int number)) g_EWPlayer[player].HudPlayer.iSheetMax = number;
					}
					if (!string.IsNullOrEmpty(sPlayerInfoFormat))
					{
						if (Int32.TryParse(sPlayerInfoFormat, out int number)) g_EWPlayer[player].PFormatPlayer = number;
					}

					if (!string.IsNullOrEmpty(sUsePriority)) g_EWPlayer[player].UsePriorityPlayer.Activate = !string.Equals(sUsePriority, "0");
					else g_EWPlayer[player].UsePriorityPlayer.Activate = true;
				}
			}catch (Exception ex) { Console.WriteLine(ex); }
		}

		public static void ShowHud()
		{
			EW.UpdateTime();
			Utilities.GetPlayers().ForEach(player =>
			{
				if (player.IsValid && CheckDictionary(player) && g_EWPlayer[player].HudPlayer != null) g_EWPlayer[player].HudPlayer.ConstructString(player);
			});
		}

		public static bool IsGameUI(CEntityInstance entity)
		{
			if (entity != null && entity.IsValid && string.Equals(entity.DesignerName, "logic_case") && !string.IsNullOrEmpty(entity.PrivateVScripts) && string.Equals(entity.PrivateVScripts.ToLower(), "game_ui")) return true;
			return false;
		}

		public static string ConvertSteamID64ToSteamID(string steamId64)
		{
			if (ulong.TryParse(steamId64, out var communityId) && communityId > 76561197960265728)
			{
				var authServer = (communityId - 76561197960265728) % 2;
				var authId = (communityId - 76561197960265728 - authServer) / 2;
				return $"STEAM_0:{authServer}:{authId}";
			}
			return null;
		}

		public static bool IsPlayerAlive(CCSPlayerController controller)
		{
			if (controller.Slot == 32766) return false;

			if (controller.LifeState == (byte)LifeState_t.LIFE_ALIVE || controller.PawnIsAlive) return true;
			else return false;
		}

		public static float Distance(CounterStrikeSharp.API.Modules.Utils.Vector point1, CounterStrikeSharp.API.Modules.Utils.Vector point2)
		{
			float dx = point2.X - point1.X;
			float dy = point2.Y - point1.Y;
			float dz = point2.Z - point1.Z;

			return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
		}
	}
}
