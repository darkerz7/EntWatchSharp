using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json;
using ClientPrefsAPI;
using EntWatchSharp.Items;
using EntWatchSharp.Helpers;
using EntWatchSharp.Modules.Eban;
using EntWatchSharp.Modules;

namespace EntWatchSharp
{
    static class EW
	{
		public static double fGameTime;
		public static List<ItemConfig> g_ItemConfig = new List<ItemConfig>();
		public static List<Item> g_ItemList = new List<Item>();
		public static Scheme g_Scheme = new Scheme();
		public static bool g_CfgLoaded = false;
		
		public static bool g_bAPI = false;
		public static IClientPrefsApi? _CP_api;

		public static Dictionary<CCSPlayerController, EbanPlayer> g_BannedPlayer = new Dictionary<CCSPlayerController, EbanPlayer>();
		public static Dictionary<CCSPlayerController, UHud> g_HudPlayer = new Dictionary<CCSPlayerController, UHud>();
		public static Dictionary<CCSPlayerController, UsePriority> g_UsePriorityPlayer = new Dictionary<CCSPlayerController, UsePriority>();
		public static List<OfflineBan> g_OfflinePlayer = new List<OfflineBan>();

		public static CounterStrikeSharp.API.Modules.Timers.Timer g_Timer = null;
		public static CounterStrikeSharp.API.Modules.Timers.Timer g_TimerRetryDB = null;
		public static CounterStrikeSharp.API.Modules.Timers.Timer g_TimerUnban = null;

		public static bool CheckDictionary(CCSPlayerController player, object dictionary)
		{
			if (dictionary is Dictionary<CCSPlayerController, EbanPlayer>)
			{
				if (!g_BannedPlayer.ContainsKey(player))
					return g_BannedPlayer.TryAdd(player, new EbanPlayer());
				return true;
			}
			if (dictionary is Dictionary<CCSPlayerController, UHud>)
			{
				if (!g_HudPlayer.ContainsKey(player))
					return g_HudPlayer.TryAdd(player, new HudNull(player));
				return true;
			}
			if (dictionary is Dictionary<CCSPlayerController, UsePriority>)
			{
				if (!g_UsePriorityPlayer.ContainsKey(player))
					return g_UsePriorityPlayer.TryAdd(player, new UsePriority(player));
				return true;
			}

			return false;
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
					if (ItemTest.thisItemConfig(iHammerID))
					{
						Item cNewItem = new Item(ItemTest, weapon);
						g_ItemList.Add(cNewItem);
						return true;
					}
				}
			}
			catch (Exception){	}
			return false;
		}

		public static CEntityInstance? EntityParentRecursive(CEntityInstance entity)
		{
			if (entity == null || !entity.IsValid) return null;

			if (entity.DesignerName.Contains("weapon_")) return entity;

			var baseentity = new CBaseEntity(entity.Handle);
			if (baseentity == null || !baseentity.IsValid) return null;
			CEntityInstance? Owner = baseentity.CBodyComponent?.SceneNode?.PParent?.Owner;
			if (Owner == null || !Owner.IsValid) return null;
			if (Owner.Entity != null && Owner.Entity.Name != null && Owner.Entity.Name.CompareTo("") != 0)
			{
				var ownerentity = baseentity.CBodyComponent.SceneNode.PParent.Owner;
				if (ownerentity == null || !ownerentity.IsValid) return null;
				return EntityParentRecursive(ownerentity);
			}
			return null;
		}

		public static int IndexItem(uint weaponindex)
		{
			for(int i = 0; i < g_ItemList.Count; i++)
			{
				Item ItemTest = g_ItemList[i];
				if (ItemTest.thisItem(weaponindex)) return i;
			}
			return -1;
		}

		public static async void LoadClientPrefs(CCSPlayerController? player)
		{
			try
			{
				string sHUDType = g_bAPI ? await _CP_api.GetClientCookie(player.SteamID.ToString(), "EW_HUD_Type") : "3";
				string sHUDPos = g_bAPI ? await _CP_api.GetClientCookie(player.SteamID.ToString(), "EW_HUD_Pos") : "50_50_50";
				string sHUDRefresh = g_bAPI ? await _CP_api.GetClientCookie(player.SteamID.ToString(), "EW_HUD_Refresh") : "3";
				string sHUDSheet = g_bAPI ? await _CP_api.GetClientCookie(player.SteamID.ToString(), "EW_HUD_Sheet") : "5";
				if (player.IsValid && CheckDictionary(player, g_HudPlayer))
				{
					if (!string.IsNullOrEmpty(sHUDPos))
					{
						try
						{
							string[] Pos = sHUDPos.Split(new char[] { '_' });
							if (Pos[0] != null && Pos[1] != null && Pos[2] != null)
							{
								g_HudPlayer[player].vecEntity.X = Int32.Parse(Pos[0]);
								g_HudPlayer[player].vecEntity.Y = Int32.Parse(Pos[1]);
								g_HudPlayer[player].vecEntity.Z = Int32.Parse(Pos[2]);
							}
						}
						catch { }
					}
					if (!string.IsNullOrEmpty(sHUDType))
					{
						int number;
						if (!Int32.TryParse(sHUDType, out number)) number = 3;
						SwitchHud(player, number);
					}
					if (!string.IsNullOrEmpty(sHUDRefresh))
					{
						int number;
						if (Int32.TryParse(sHUDRefresh, out number)) g_HudPlayer[player].iRefresh = number;
					}
					if (!string.IsNullOrEmpty(sHUDSheet))
					{
						int number;
						if (Int32.TryParse(sHUDSheet, out number)) g_HudPlayer[player].iSheetMax = number;
					}
				}
				string sUsePriority = g_bAPI ? await _CP_api.GetClientCookie(player.SteamID.ToString(), "EW_Use_Priority") : "1";
				if (player.IsValid && CheckDictionary(player, g_UsePriorityPlayer))
				{
					if (!string.IsNullOrEmpty(sUsePriority)) g_UsePriorityPlayer[player].Activate = sUsePriority.CompareTo("0") != 0;
					else g_UsePriorityPlayer[player].Activate = true;
				}
			}catch (Exception ex) { Console.WriteLine(ex); }
		}

		public static void RemoveEntityHud(CCSPlayerController? player)
		{
			var plHud = g_HudPlayer[player];
			if (plHud is HudWorldText && ((HudWorldText)plHud).Entity != null)
			{
				if (((HudWorldText)plHud).Entity.IsValid)
				{
					//((HudWorldText)plHud).Entity.AcceptInput("Kill");
					((HudWorldText)plHud).Entity.Remove();
				}
				((HudWorldText)plHud).Entity = null;
			}
		}

		public static void SwitchHud(CCSPlayerController? player, int number)
		{
			Server.NextFrame(() =>
			{
				if (CheckDictionary(player, g_HudPlayer))
				{
					RemoveEntityHud(player);

					var LastCfg = g_HudPlayer[player];

					if (g_HudPlayer.Remove(player))
					{
						switch (number)
						{
							case 0: g_HudPlayer.TryAdd(player, new HudNull(player)); break;
							case 1: g_HudPlayer.TryAdd(player, new HudCenter(player)); break;
							case 2: g_HudPlayer.TryAdd(player, new HudAlert(player)); break;
							case 3: g_HudPlayer.TryAdd(player, new HudWorldText(player)); break;
							default: g_HudPlayer.TryAdd(player, new HudNull(player)); break;
						}
					}
					if (CheckDictionary(player, g_HudPlayer))
					{
						g_HudPlayer[player].vecEntity = LastCfg.vecEntity;
						g_HudPlayer[player].iSheetMax = LastCfg.iSheetMax;
						g_HudPlayer[player].iRefresh = LastCfg.iRefresh;
						if (g_HudPlayer[player] is HudWorldText && ((HudWorldText)g_HudPlayer[player]).Entity == null) ((HudWorldText)g_HudPlayer[player]).CreateHud();
					}
				}
			});
		}

		public static void ShowHud()
		{
			EW.UpdateTime();
			Utilities.GetPlayers().ForEach(player =>
			{
				if (player.IsValid && CheckDictionary(player, g_HudPlayer) && g_HudPlayer[player] != null) EW.g_HudPlayer[player].ConstructString();
			});
		}

		public static bool IsGameUI(CEntityInstance entity)
		{
			if (entity != null && entity.IsValid && entity.DesignerName.CompareTo("logic_case") == 0 && !string.IsNullOrEmpty(entity.PrivateVScripts) && entity.PrivateVScripts.ToLower().CompareTo("game_ui") == 0) return true;
			return false;
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
