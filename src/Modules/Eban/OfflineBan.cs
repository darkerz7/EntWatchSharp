using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using EntWatchSharp.Helpers;

namespace EntWatchSharp.Modules.Eban
{
	public class OfflineBan
	{
		public int UserID;
		public string Name;
		public string SteamID;
		public string LastItem;
		public double TimeStamp;
		public double TimeStamp_Start;
		public bool Online;
		public uint Immutity;
		public CCSPlayerController Player;

		public OfflineBan(){}
	}

	public static class OfflineFunc
	{
		static OfflineBan CreateOrFind(CCSPlayerController UserID)
		{
			OfflineBan offlineplayer = null;
			foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
			{
				if (OfflineTest.SteamID.CompareTo(EW.ConvertSteamID64ToSteamID(UserID.SteamID.ToString())) == 0)
				{
					offlineplayer = OfflineTest;
					break;
				}
			}
			if (offlineplayer == null)
			{
				offlineplayer = new OfflineBan();
				EW.g_OfflinePlayer.Add(offlineplayer);
			}
			offlineplayer.UserID = UserID.UserId ?? 0;
			offlineplayer.Name = UserID.PlayerName;
			offlineplayer.SteamID = EW.ConvertSteamID64ToSteamID(UserID.SteamID.ToString());
			offlineplayer.Immutity = AdminManager.GetPlayerImmunity(UserID);
			return offlineplayer;
		}
#nullable enable
		public static void PlayerConnectFull(CCSPlayerController? UserID)
#nullable disable
		{
			if (UserID == null || !UserID.IsValid ||UserID.IsBot) return;
			OfflineBan OfflinePlayer = CreateOrFind(UserID);
			OfflinePlayer.Player = UserID;
			OfflinePlayer.Online = true;
		}
#nullable enable
		public static void PlayerDisconnect(CCSPlayerController? UserID)
#nullable disable
		{
			if (UserID == null || !UserID.IsValid || UserID.IsBot) return;
			OfflineBan OfflinePlayer = CreateOrFind(UserID);
			OfflinePlayer.TimeStamp_Start = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			OfflinePlayer.TimeStamp = OfflinePlayer.TimeStamp_Start + Cvar.OfflineClearTime * 60;
			OfflinePlayer.Player = null;
			OfflinePlayer.Online = false;
		}
		public static void TimeToClear()
		{
			double CurrentTime = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
			{
				if(!OfflineTest.Online && OfflineTest.TimeStamp < CurrentTime) EW.g_OfflinePlayer.Remove(OfflineTest);
			}
		}

		public static OfflineBan FindTarget(CCSPlayerController admin, string sTarget, bool bConsole)
		{
			uint iAdminImmunity = AdminManager.GetPlayerImmunity(admin);
			OfflineBan target = null;
			if (sTarget.ToLower().StartsWith("#steam_"))
			{
				string sTargetSteamID = sTarget.Substring(1).ToLower();
				//steamid
				foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
				{
					if (!OfflineTest.Online && OfflineTest.SteamID.ToLower().CompareTo(sTargetSteamID) == 0)
					{
						target = OfflineTest;
						break;
					}
				}
			}
			else if (sTarget[0] == '#')
			{
				//userid
				if (!int.TryParse(sTarget.Substring(1), out int iUID))
				{
					UI.EWReplyInfo(admin, "Reply.Must_be_an_integer", bConsole);
					return null;
				}
				foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
				{
					if (!OfflineTest.Online && OfflineTest.UserID == iUID)
					{
						target = OfflineTest;
						break;
					}
				}
			}
			else
			{
				//name
				int iCount = 0;
				foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
				{
					if (!OfflineTest.Online && OfflineTest.Name.ToLower().Contains(sTarget.ToLower()) && (admin == null || iAdminImmunity > OfflineTest.Immutity))
					{
						target = OfflineTest;
						iCount++;
					}
				}
				if (iCount > 1)
				{
					UI.EWReplyInfo(admin, "Reply.More_than_one_client_matched", bConsole);
					return null;
				}
			}
			if (admin == null || target != null && iAdminImmunity > target.Immutity)
			{
				return target;
			}

			return null;
		}
	}
}
