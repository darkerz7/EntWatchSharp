using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CS2_GameHUDAPI;
using EntWatchSharp.Helpers;
using EntWatchSharp.Items;
using EntWatchSharp.Modules.Eban;
using EntWatchSharpAPI;
using PlayerSettings;
using System.Globalization;
using System.Text.Json;

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
		public static string g_WeaponName = "EntWatchItem_";
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
			try
			{
				g_WeaponName = $"EntWatchItem_{Server.MapName}";
				if (g_WeaponName.Length > 64) g_WeaponName = g_WeaponName[..64];
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
			catch (Exception e)
			{
				UI.EWSysInfo("Info.Error", 15, $"Bad Config file for {(Cvar.LowerMapname ? Server.MapName.ToLower() : Server.MapName)}!");
				UI.EWSysInfo("Info.Error", 15, $"{e.Message}");
				g_CfgLoaded = false;
			}
		}

		public static void LoadScheme()
		{
			try
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
			catch (Exception e)
			{
				UI.EWSysInfo("Info.Error", 15, $"Bad Scheme file for {Cvar.PathScheme}!");
				UI.EWSysInfo("Info.Error", 15, $"{e.Message}");
				g_CfgLoaded = false;
			}
		}

		public static bool WeaponIsItem(CEntityInstance entity)
		{
			var weapon = new CCSWeaponBase(entity.Handle);
			if (weapon == null || !weapon.IsValid) return false;
			try
			{
				foreach (ItemConfig ItemTest in g_ItemConfig.ToList())
				{
					if (ItemTest.ThisItemConfig(weapon.UniqueHammerID))
					{
						Item cNewItem = new(ItemTest, weapon);
						g_ItemList.Add(cNewItem);
						weapon.As<CEconEntity>().AttributeManager.Item.CustomName = EW.g_WeaponName;
						return true;
					}
				}
			}
			catch (Exception){	}
			return false;
		}

		public static void DropSpecialWeapon(CCSPlayerController player)
		{
			if (player.IsValid && player.PlayerPawn.Value != null && player.PlayerPawn.Value.IsValid && player.PlayerPawn.Value.AbsOrigin != null && player.PlayerPawn.Value.WeaponServices != null)
			{
				try
				{
					System.Numerics.Vector3 vecPos = (System.Numerics.Vector3)player.PlayerPawn.Value.AbsOrigin with { Z = player.PlayerPawn.Value.AbsOrigin.Z + 70 };
					foreach (var weapon in player.PlayerPawn.Value.WeaponServices.MyWeapons)
					{
						if (!weapon.IsValid || weapon.Value == null || string.IsNullOrEmpty(weapon.Value.UniqueHammerID)) continue;

						CBasePlayerWeapon wpn = new(weapon.Value.Handle);

						player.PlayerPawn.Value.WeaponServices.ActiveWeapon.Raw = weapon.Raw;
						player.DropActiveWeapon();

						//Fix for item dropping underground
						Server.NextWorldUpdate(() =>
						{
							if (wpn != null && wpn.IsValid) wpn.Teleport(vecPos);
						});
					}
				} catch { }
			}
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
			Task.Run(() =>
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
			});
		}

		public static void ShowHud()
		{
			EW.UpdateTime();
			/*foreach(var pair in EW.g_EWPlayer)
			{
				if (g_EWPlayer[pair.Key].HudPlayer != null) g_EWPlayer[pair.Key].HudPlayer.ConstructString(pair.Key);
			}*/
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

        public static (List<CCSPlayerController> players, string targetname, ProcessTargetResultFlag result) FindTargets(CCSPlayerController? player, string targetString, bool nobots, bool immunity, bool aliveonly)
        {
            var filter = ProcessTargetFilterFlag.None;

            if (nobots)
                filter |= ProcessTargetFilterFlag.FilterNoBots;

            if (!immunity)
                filter |= ProcessTargetFilterFlag.FilterNoImmunity;

			if (aliveonly)
				filter |= ProcessTargetFilterFlag.FilterAlive;

            ProcessTargetResultFlag result;
            if ((result = Target.ProcessTargetString(player, targetString, filter, true, out var targetname, out var players)) == ProcessTargetResultFlag.TargetFound)
                return (players, targetname, result);

            return ([], "", result);
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

		public static float Distance(System.Numerics.Vector3 point1, System.Numerics.Vector3 point2)
		{
			float dx = point2.X - point1.X;
			float dy = point2.Y - point1.Y;
			float dz = point2.Z - point1.Z;

			return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
		}
	}
}
