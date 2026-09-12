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
		public int PFormatPlayer;

		public EWPlayer()
		{
			BannedPlayer = new EbanPlayer();
			HudPlayer = new UHud();
			UsePriorityPlayer = new UsePriority();
			PrivilegePlayer = new Privilege();
			PFormatPlayer = Cvar.PlayerFormat;
		}
	}
}
