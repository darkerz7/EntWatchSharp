using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using EntWatchSharp.Helpers;
using EntWatchSharp.Items;

namespace EntWatchSharp
{
	public partial class EntWatchSharp: BasePlugin
	{
		public FakeConVar<bool> FakeCvar_teamonly = new("ewc_teamonly", "Enable/Disable team only mode", true, flags: ConVarFlags.FCVAR_NOTIFY, new RangeValidator<bool>(false, true));
		public FakeConVar<byte> FakeCvar_adminchat = new("ewc_adminchat", "Change Admin Chat Mode (0 - All Messages, 1 - Only Pickup/Drop Items, 2 - Nothing)", 0, flags: ConVarFlags.FCVAR_NOTIFY, new RangeValidator<byte>(0, 2));
		public FakeConVar<byte> FakeCvar_adminhud = new("ewc_adminhud", "Change Admin Hud Mode (0 - All Items, 1 - Only Item Name, 2 - Nothing)", 0, flags: ConVarFlags.FCVAR_NOTIFY, new RangeValidator<byte>(0, 2));

		public FakeConVar<bool> FakeCvar_blockepick = new("ewc_blockepick", "Block players from using E key to grab items", true, flags: ConVarFlags.FCVAR_NOTIFY, new RangeValidator<bool>(false, true));
		public FakeConVar<double> FakeCvar_delay = new("ewc_delay_use", "Change delay before use", 1.0, flags: ConVarFlags.FCVAR_NOTIFY, new RangeValidator<double>(0.0, 60.0));
		public FakeConVar<bool> FakeCvar_globalblock = new("ewc_globalblock", "Blocks the pickup of any items by players", false, flags: ConVarFlags.FCVAR_NOTIFY, new RangeValidator<bool>(false, true));
		public FakeConVar<byte> FakeCvar_display_ability = new("ewc_display_ability", "Count of the abilities to display on the HUD", 4, flags: ConVarFlags.FCVAR_NOTIFY, new RangeValidator<byte>(0, 4));
		public FakeConVar<bool> FakeCvar_use_priority = new("ewc_use_priority", "Enable/Disable forced pressing of the button", true, flags: ConVarFlags.FCVAR_NOTIFY, new RangeValidator<bool>(false, true));
		public FakeConVar<bool> FakeCvar_display_mapcommands = new("ewc_display_mapcommands", "Enable/Disable display of item changes", true, flags: ConVarFlags.FCVAR_NOTIFY, new RangeValidator<bool>(false, true));

		public FakeConVar<string> FakeCvar_path_scheme = new("ewc_path_scheme", "Path with filename for the scheme", "addons/entwatch/scheme/default.json", flags: ConVarFlags.FCVAR_NONE);
		public FakeConVar<string> FakeCvar_path_cfg = new("ewc_path_cfg", "Directory for configs", "addons/entwatch/maps/", flags: ConVarFlags.FCVAR_NONE);
		public FakeConVar<bool> FakeCvar_lower_mapname = new("ewc_lower_mapname", "Automatically lowercase map name", false, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));
		public FakeConVar<bool> FakeCvar_triggeronce = new("ewc_triggeronce", "Exclude trigger_once from ban check", true, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));

		public FakeConVar<bool> FakeCvar_glow_spawn = new("ewc_glow_spawn", "Enable/Disable the glow after Spawn Items", true, flags: ConVarFlags.FCVAR_NOTIFY, new RangeValidator<bool>(false, true));
		public FakeConVar<bool> FakeCvar_glow_particle = new("ewc_glow_particle", "Enable/Disable the glow using a particle", true, flags: ConVarFlags.FCVAR_NOTIFY, new RangeValidator<bool>(false, true));

		public FakeConVar<int> FakeCvar_bantime = new("ewc_bantime", "Default ban time. 0 - Permanent", 0, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 43200));
		public FakeConVar<int> FakeCvar_banlong = new("ewc_banlong", "Max ban time with once @css/ew_ban privilege", 720, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 1440000));
		public FakeConVar<string> FakeCvar_banreason = new("ewc_banreason", "Default ban reason", "Trolling", flags: ConVarFlags.FCVAR_NONE);
		public FakeConVar<string> FakeCvar_unbanreason = new("ewc_unbanreason", "Default unban reason", "Giving another chance", flags: ConVarFlags.FCVAR_NONE);
		public FakeConVar<bool> FakeCvar_keepexpiredban = new("ewc_keep_expired_ban", "Enable/Disable keep expired bans", true, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<bool>(false, true));
		public FakeConVar<int> FakeCvar_offline_clear_time = new("ewc_offline_clear_time", "Time during which data is stored (1-240)", 30, flags: ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 240));

		private void RegisterCVARS()
		{
			Cvar.TeamOnly = FakeCvar_teamonly.Value;
			Cvar.AdminChat = FakeCvar_adminchat.Value;
			Cvar.AdminHud = FakeCvar_adminhud.Value;

			Cvar.BlockEPickup = FakeCvar_blockepick.Value;
			Cvar.Delay = FakeCvar_delay.Value;
			Cvar.GlobalBlock = FakeCvar_globalblock.Value;
			Cvar.DisplayAbility = FakeCvar_display_ability.Value;
			Cvar.UsePriority = FakeCvar_use_priority.Value;
			Cvar.DisplayMapCommands = FakeCvar_display_mapcommands.Value;

			Cvar.PathScheme = FakeCvar_path_scheme.Value;
			Cvar.PathCfg = FakeCvar_path_cfg.Value;
			Cvar.LowerMapname = FakeCvar_lower_mapname.Value;
			Cvar.TriggerOnceException = FakeCvar_triggeronce.Value;

			Cvar.GlowSpawn = FakeCvar_glow_spawn.Value;
			Cvar.GlowParticle = FakeCvar_glow_particle.Value;

			Cvar.BanTime = FakeCvar_bantime.Value;
			Cvar.BanLong = FakeCvar_banlong.Value;
			Cvar.BanReason = FakeCvar_banreason.Value;
			Cvar.UnBanReason = FakeCvar_unbanreason.Value;
			Cvar.KeepExpiredBan = FakeCvar_keepexpiredban.Value;
			Cvar.OfflineClearTime = FakeCvar_offline_clear_time.Value;

			FakeCvar_teamonly.ValueChanged += (sender, value) =>
			{
				Cvar.TeamOnly = value;
				UI.CvarChangeNotify(FakeCvar_teamonly.Name, value.ToString(), FakeCvar_teamonly.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_adminchat.ValueChanged += (sender, value) =>
			{
				Cvar.AdminChat = value;
				UI.CvarChangeNotify(FakeCvar_adminchat.Name, value.ToString(), FakeCvar_adminchat.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_adminhud.ValueChanged += (sender, value) =>
			{
				Cvar.AdminHud = value;
				UI.CvarChangeNotify(FakeCvar_adminhud.Name, value.ToString(), FakeCvar_adminhud.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};

			FakeCvar_blockepick.ValueChanged += (sender, value) =>
			{
				Cvar.BlockEPickup = value;
				UI.CvarChangeNotify(FakeCvar_blockepick.Name, value.ToString(), FakeCvar_blockepick.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_delay.ValueChanged += (sender, value) =>
			{
				Cvar.Delay = value;
				UI.CvarChangeNotify(FakeCvar_delay.Name, value.ToString(), FakeCvar_delay.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_globalblock.ValueChanged += (sender, value) =>
			{
				Cvar.GlobalBlock = value;
				foreach(Item ItemTest in EW.g_ItemList.ToList()) ItemTest.WeaponHandle.CanBePickedUp = Cvar.GlobalBlock;
				UI.CvarChangeNotify(FakeCvar_globalblock.Name, value.ToString(), FakeCvar_globalblock.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_display_ability.ValueChanged += (sender, value) =>
			{
				Cvar.DisplayAbility = value;
				UI.CvarChangeNotify(FakeCvar_display_ability.Name, value.ToString(), FakeCvar_display_ability.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_use_priority.ValueChanged += (sender, value) =>
			{
				Cvar.UsePriority = value;
				UI.CvarChangeNotify(FakeCvar_use_priority.Name, value.ToString(), FakeCvar_use_priority.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_display_mapcommands.ValueChanged += (sender, value) =>
			{
				Cvar.DisplayMapCommands = value;
				UI.CvarChangeNotify(FakeCvar_display_mapcommands.Name, value.ToString(), FakeCvar_display_mapcommands.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};

			FakeCvar_path_scheme.ValueChanged += (sender, value) =>
			{
				if (!string.IsNullOrEmpty(value))
				{
					Cvar.PathScheme = value.Replace("\"", "");
					UI.CvarChangeNotify(FakeCvar_path_scheme.Name, Cvar.PathScheme.ToString(), FakeCvar_path_scheme.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
				}
			};
			FakeCvar_path_cfg.ValueChanged += (sender, value) =>
			{
				if (!string.IsNullOrEmpty(value))
				{
					Cvar.PathCfg = value.Replace("\"", "");
					UI.CvarChangeNotify(FakeCvar_path_cfg.Name, Cvar.PathCfg.ToString(), FakeCvar_path_cfg.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
				}
			};
			FakeCvar_lower_mapname.ValueChanged += (sender, value) =>
			{
				Cvar.LowerMapname = value;
				UI.CvarChangeNotify(FakeCvar_lower_mapname.Name, value.ToString(), FakeCvar_lower_mapname.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_triggeronce.ValueChanged += (sender, value) =>
			{
				Cvar.TriggerOnceException = value;
				UI.CvarChangeNotify(FakeCvar_triggeronce.Name, value.ToString(), FakeCvar_triggeronce.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};

			FakeCvar_glow_spawn.ValueChanged += (sender, value) =>
			{
				Cvar.GlowSpawn = value;
				UI.CvarChangeNotify(FakeCvar_glow_spawn.Name, value.ToString(), FakeCvar_glow_spawn.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_glow_particle.ValueChanged += (sender, value) =>
			{
				Cvar.GlowParticle = value;
				UI.CvarChangeNotify(FakeCvar_glow_particle.Name, value.ToString(), FakeCvar_glow_particle.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};

			FakeCvar_bantime.ValueChanged += (sender, value) =>
			{
				if(value >= 0 && value <= 43200) Cvar.BanTime = value;
				else Cvar.BanTime = 0;
				UI.CvarChangeNotify(FakeCvar_bantime.Name, value.ToString(), FakeCvar_bantime.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_banlong.ValueChanged += (sender, value) =>
			{
				if (value >= 1 && value <= 1440000) Cvar.BanLong = value;
				else Cvar.BanLong = 720;
				UI.CvarChangeNotify(FakeCvar_banlong.Name, value.ToString(), FakeCvar_banlong.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_banreason.ValueChanged += (sender, value) =>
			{
				if (!string.IsNullOrEmpty(value))
				{
					Cvar.BanReason = value.Replace("\"", "");
					UI.CvarChangeNotify(FakeCvar_banreason.Name, Cvar.BanReason.ToString(), FakeCvar_banreason.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
				}
			};
			FakeCvar_unbanreason.ValueChanged += (sender, value) =>
			{
				if (!string.IsNullOrEmpty(value))
				{
					Cvar.UnBanReason = value.Replace("\"", "");
					UI.CvarChangeNotify(FakeCvar_unbanreason.Name, Cvar.UnBanReason.ToString(), FakeCvar_unbanreason.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
				}
			};
			FakeCvar_keepexpiredban.ValueChanged += (sender, value) =>
			{
				Cvar.KeepExpiredBan = value;
				UI.CvarChangeNotify(FakeCvar_keepexpiredban.Name, value.ToString(), FakeCvar_keepexpiredban.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};
			FakeCvar_offline_clear_time.ValueChanged += (sender, value) =>
			{
				if (value >= 1 && value <= 240) Cvar.OfflineClearTime = value;
				else Cvar.OfflineClearTime = 30;
				UI.CvarChangeNotify(FakeCvar_offline_clear_time.Name, value.ToString(), FakeCvar_offline_clear_time.Flags.HasFlag(ConVarFlags.FCVAR_NOTIFY));
			};

			RegisterFakeConVars(typeof(ConVar));
		}
	}

	static class Cvar
	{
		public static bool TeamOnly;
		public static byte AdminChat;
		public static byte AdminHud;

		public static bool BlockEPickup;
		public static double Delay;
		public static bool GlobalBlock;
		public static byte DisplayAbility;
		public static bool UsePriority;
		public static bool DisplayMapCommands;

		public static string PathScheme;
		public static string PathCfg;
		public static bool LowerMapname;
		public static bool TriggerOnceException;

		public static bool GlowSpawn;
		public static bool GlowParticle;

		public static int BanTime;
		public static int BanLong;
		public static string BanReason;
		public static string UnBanReason;
		public static bool KeepExpiredBan;
		public static int OfflineClearTime;
	}
}
