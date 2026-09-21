using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using EntWatchSharp.Helpers;
using EntWatchSharp.Items;
using EntWatchSharp.Modules.Eban;
using EntWatchSharpAPI;
using PlayerSettings;
using System.Collections.Frozen;
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
        readonly static HashSet<(string, string)> g_HookedOutputs = [];
        readonly static HashSet<string> g_HookOutput_TrackedClasses = [];

#nullable enable
		public static CCSCustomHudLayout? g_CustomHudLayout;
        public static ISettingsApi? _PlayerSettingsAPI;
		public readonly static PluginCapability<ISettingsApi?> _PlayerSettingsAPICapability = new("settings:nfcore");
#nullable disable

		public static IEntWatchSharpAPI _EW_api;
		public static EWAPI g_cAPI = null;

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
            if (g_CfgLoaded && g_ItemConfig is { })
            {
                foreach (ItemConfig ItemTest in g_ItemConfig.ToList())
                    foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
                        AddHookOutput(AbilityTest.ButtonClass, AbilityTest.Event);
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
                        //Auto block after adding to the list
                        if (cNewItem.BlockPickup || Cvar.GlobalBlock) cNewItem.WeaponHandle.CanBePickedUp = false;
                        else cNewItem.WeaponHandle.CanBePickedUp = true;
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
					string sHUDShow = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_HUD_Show", "1") : "1";
					string sHUDSize = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_HUD_SizeType", "1") : "1";
                    string sHUDPos = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_HUD_PosNum", "0") : "0";
                    string sHUDRefresh = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_HUD_Refresh", "3") : "3";
					string sPlayerInfoFormat = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_PInfo_Format", $"{Cvar.PlayerFormat}") : $"{Cvar.PlayerFormat}";

					string sUsePriority = _PlayerSettingsAPI != null ? _PlayerSettingsAPI.GetPlayerSettingsValue(player, "EW_Use_Priority", "1") : "1";
					if (player.IsValid && CheckDictionary(player))
					{
						if (!string.IsNullOrEmpty(sHUDSize))
						{
							if (!byte.TryParse(sHUDSize, out byte number)) number = 1;
							g_EWPlayer[player].HudPlayer.iSize = number;
						}
                        if (!string.IsNullOrEmpty(sHUDPos))
                        {
                            if (!byte.TryParse(sHUDPos, out byte number)) number = 0;
							Server.NextWorldUpdate(() =>
							{
								if (player.IsValid && CheckDictionary(player)) g_EWPlayer[player].HudPlayer.ChangePosition(player, number);
                            });
                        }
                        if (!string.IsNullOrEmpty(sHUDShow)) g_EWPlayer[player].HudPlayer.bShow = !string.Equals(sHUDShow, "0");
						if (!string.IsNullOrEmpty(sHUDRefresh))
						{
							if (Int32.TryParse(sHUDRefresh, out int number)) g_EWPlayer[player].HudPlayer.iRefresh = number;
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
			Utilities.GetPlayers().ForEach(player =>
			{
				if (player.IsValid && CheckDictionary(player) && g_EWPlayer[player].HudPlayer != null) g_EWPlayer[player].HudPlayer.UpdateHUD(player);
			});
		}

		public static bool HookEntityOutput_Handler(CEntityInstance cActivator, CEntityInstance cCaller, string sOutput)
		{
            if (!g_CfgLoaded) return true;

			if (HookOutput_CheckTrackedClasses(cCaller.DesignerName) &&
				g_HookedOutputs.Contains((cCaller.DesignerName, sOutput.ToLower())))
				return EntWatchSharp.OnButtonPressed(cActivator, cCaller);

            return true;
        }

        public static void AddHookOutput(string classname, string output)
        {
            if (string.IsNullOrEmpty(output) || string.IsNullOrEmpty(classname)) return;

            string sClassname_Lower = classname.ToLower();

            if (HookOutput_ValidClassList.Contains(sClassname_Lower))
            {
                string sOutput_Lower = output.ToLower();
                if (g_HookedOutputs.Add((sClassname_Lower, sOutput_Lower)))
                {
                    g_HookOutput_TrackedClasses.Add(sClassname_Lower);
                }
            }
        }

		public static void ReloadHookOutputs()
		{
            g_HookOutput_TrackedClasses.Clear();
            g_HookedOutputs.Clear();

            AddHookOutput("func_button", "OnPressed");
            AddHookOutput("func_rot_button", "OnPressed");
            AddHookOutput("func_door", "OnOpen");
            AddHookOutput("func_door_rotating", "OnOpen");
            AddHookOutput("func_physbox", "OnPlayerUse");
        }

        public static bool HookOutput_CheckTrackedClasses(string classname)
        {
            if (string.IsNullOrEmpty(classname)) return false;

            return g_HookOutput_TrackedClasses.Contains(classname.ToLower());
        }

        static readonly FrozenSet<string> HookOutput_ValidClassList = [
            "func_button",
            "func_physbox",
            "func_door",
            "func_rot_button",
            "func_door_rotating",
            "logic_case",
            "logic_relay",
            "logic_timer",
            "logic_branch_listener",
            "logic_branch",
            "logic_compare",
            "math_counter",
            "trigger_gravity",
            "trigger_hurt",
            "trigger_look",
            "trigger_multiple",
            "trigger_once",
            "trigger_push",
            "trigger_teleport",
            "trigger_wind",
            "env_entity_maker",
            "point_template",
            "filter_activator_attribute_int",
            "filter_activator_class",
            "filter_activator_context",
            "filter_activator_model",
            "filter_activator_name",
            "filter_multi",
            "filter_activator_team",
            "func_breakable"
            ];

#nullable enable
        public static CCSCustomHudLayout? GetorCreateHudLayout()
#nullable disable
        {
            if (g_CustomHudLayout is { IsValid: true }) return g_CustomHudLayout;
			if (Utilities.CreateEntityByName<CCSCustomHudLayout>("custom_hud_layout") is { IsValid: true } customhud)
			{
				customhud.StrLayout = "panorama/layout/custom_game/entwatch_dz.vxml_c";
				return g_CustomHudLayout = customhud;
			}
            return null;
        }

        public static void RemoveHudLayout()
        {
            g_CustomHudLayout = null;
        }

        public static bool IsGameUI(CEntityInstance entity)
		{
			if (entity != null && entity.IsValid && string.Equals(entity.DesignerName, "logic_case") && !string.IsNullOrEmpty(entity.PrivateVScripts) && string.Equals(entity.PrivateVScripts.ToLower(), "game_ui")) return true;
			return false;
		}

#nullable enable
        public static (List<CCSPlayerController> players, string targetname, ProcessTargetResultFlag result) FindTargets(CCSPlayerController? player, string targetString, bool nobots, bool immunity, bool aliveonly)
#nullable disable
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

        public static string ConvertSteamID64ToSteamID(ulong steamId64)
		{
			if (steamId64 > 76561197960265728)
			{
				var authServer = (steamId64 - 76561197960265728) % 2;
				var authId = (steamId64 - 76561197960265728 - authServer) / 2;
				return $"STEAM_0:{authServer}:{authId}";
			}
			return "";
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
