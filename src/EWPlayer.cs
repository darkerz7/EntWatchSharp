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

		/*public void RemoveEntityHud()
		{
			if (HudPlayer is HudWorldText hud && hud.Entity != null)
			{
				if (hud.Entity.IsValid) hud.Entity.Remove();
				hud.Entity = null;
			}
		}*/
		public void RemoveEntityHud(CCSPlayerController player)
		{
			if (EW._GH_api != null && player.IsValid)
			{
				EW._GH_api.Native_GameHUD_Remove(player, EW.HUDCHANNEL);
			}
		}

		public void SwitchHud(CCSPlayerController player, int number)
		{
			Server.NextFrame(() =>
			{
				RemoveEntityHud(player);

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
				HudPlayer.iSize = LastCfg.iSize;
				//if (HudPlayer is HudWorldText hud && hud.Entity == null) hud.CreateHud(player);
				if (HudPlayer is HudWorldText hud) hud.InitHud(player);
			});
		}
	}
}
