using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using EntWatchSharp.Modules;
using EntWatchSharp.Modules.Eban;

namespace EntWatchSharp
{
	internal class EWPlayer
	{
		public EbanPlayer BannedPlayer;
		public UHud HudPlayer;
		public UsePriority UsePriorityPlayer;
		public Privilege PrivilegePlayer;

		public EWPlayer()
		{
			BannedPlayer = new EbanPlayer();
			HudPlayer = new HudNull();
			UsePriorityPlayer = new UsePriority();
			PrivilegePlayer = new Privilege();
		}

		public void RemoveEntityHud()
		{
			if (HudPlayer is HudWorldText hud && hud.Entity != null)
			{
				if (hud.Entity.IsValid) hud.Entity.Remove();
				hud.Entity = null;
			}
		}

		public void SwitchHud(CCSPlayerController player, int number)
		{
			Server.NextFrame(() =>
			{
				RemoveEntityHud();

				var LastCfg = HudPlayer;

				HudPlayer = number switch
				{
					0 => new HudNull(),
					1 => new HudCenter(),
					2 => new HudAlert(),
					3 => new HudWorldText(),
					_ => new HudNull(),
				};
				HudPlayer.vecEntity = LastCfg.vecEntity;
				HudPlayer.colorEntity = LastCfg.colorEntity;
				HudPlayer.iSheetMax = LastCfg.iSheetMax;
				HudPlayer.iRefresh = LastCfg.iRefresh;
				if (HudPlayer is HudWorldText hud && hud.Entity == null) hud.CreateHud(player);
			});
		}
	}
}
