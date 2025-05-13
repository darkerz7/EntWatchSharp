using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using EntWatchSharp.Helpers;
using EntWatchSharpAPI;

namespace EntWatchSharp.Modules.Eban
{
    internal class EbanPlayer
    {
        public bool bBanned;

        public string sAdminName;
        public string sAdminSteamID;
        public int iDuration;
        public int iTimeStamp_Issued;
        public string sReason;

        public string sClientName;
		public string sClientSteamID;

		public bool bFixSpawnItem;

        public bool SetBan(string sBanAdminName, string sBanAdminSteamID, string sBanClientName, string sBanClientSteamID, int iBanDuration, string sBanReason)
        {
            if (!string.IsNullOrEmpty(sBanClientSteamID))
            {
                bBanned = true;
                sAdminName = sBanAdminName;
                sAdminSteamID = sBanAdminSteamID;
                sReason = sBanReason;
                if (iBanDuration < -1)
                {
                    iDuration = -1;
                    iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
					UI.EWSysInfo("Reply.Eban.Ban.Success", 6);
				}
                else if (iBanDuration == 0)
                {
                    iDuration = 0;
                    iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                }
                else
                {
                    iDuration = iBanDuration;
                    iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()) + iDuration * 60;
                }
                if(EW.g_cAPI != null)
                {
					SEWAPI_Ban apiBan = new()
					{
						bBanned = bBanned,
						sAdminName = sAdminName,
						sAdminSteamID = sAdminSteamID,
						iDuration = iDuration,
						iTimeStamp_Issued = iTimeStamp_Issued,
						sReason = sReason,
						sClientName = sClientName,
						sClientSteamID = sClientSteamID
					};
					EW.g_cAPI.OnClientBanned(apiBan);
				}
				EbanDB.BanClient(sBanClientName, sBanClientSteamID, sAdminName, sAdminSteamID, EW.g_Scheme.server_name, iDuration, iTimeStamp_Issued, sReason);
				return true;
            }
            return false;
        }

        public bool UnBan(string sUnBanAdminName, string sUnBanAdminSteamID, string sUnBanClientSteamID, string sUnbanReason)
        {
            if (!string.IsNullOrEmpty(sUnBanClientSteamID))
            {
				bBanned = false;
                if (string.IsNullOrEmpty(sUnbanReason)) sUnbanReason = "Amnesty";
				if (EW.g_cAPI != null)
				{
					SEWAPI_Ban apiBan = new()
					{
						bBanned = bBanned,
						sAdminName = sUnBanAdminName,
						sAdminSteamID = sUnBanAdminSteamID,
						iDuration = 0,
						iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
						sReason = sUnbanReason,
						sClientName = "",
						sClientSteamID = sUnBanClientSteamID
					};
					EW.g_cAPI.OnClientUnbanned(apiBan);
				}
				EbanDB.UnBanClient(sUnBanClientSteamID, sUnBanAdminName, sUnBanAdminSteamID, EW.g_Scheme.server_name, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), sUnbanReason);
				return true;
            }
            return false;
        }

        public static void GetBan(CCSPlayerController player, bool bShow = false)
        {
			EbanDB.GetBan(player, EW.g_Scheme.server_name, GetBanPlayer_Handler, bShow);
        }

        static EbanDB.GetBanPlayerFunc GetBanPlayer_Handler = (CCSPlayerController player, List<List<string>> DBQuery_Result, bool bShow) =>
        {
			if (player.IsValid && EW.CheckDictionary(player))
            {
				if (DBQuery_Result.Count > 0)
				{
					EW.g_EWPlayer[player].BannedPlayer.bBanned = true;
					EW.g_EWPlayer[player].BannedPlayer.sAdminName = DBQuery_Result[0][0];
					EW.g_EWPlayer[player].BannedPlayer.sAdminSteamID = DBQuery_Result[0][1];
					EW.g_EWPlayer[player].BannedPlayer.iDuration = Convert.ToInt32(DBQuery_Result[0][2]);
					EW.g_EWPlayer[player].BannedPlayer.iTimeStamp_Issued = Convert.ToInt32(DBQuery_Result[0][3]);
					EW.g_EWPlayer[player].BannedPlayer.sReason = DBQuery_Result[0][4];
					if (bShow)
						Server.NextFrame(() =>
						{
							UI.EWSysInfo("Info.Eban.PlayerConnect", 4, UI.PlayerInfoFormat(player)[3], EW.g_EWPlayer[player].BannedPlayer.iDuration, EW.g_EWPlayer[player].BannedPlayer.iTimeStamp_Issued, UI.PlayerInfoFormat(EW.g_EWPlayer[player].BannedPlayer.sAdminName, EW.g_EWPlayer[player].BannedPlayer.sAdminSteamID)[3], EW.g_EWPlayer[player].BannedPlayer.sReason);
						});
				}
				else
				{
					EW.g_EWPlayer[player].BannedPlayer.bBanned = false;
				}
			}
		};
#nullable enable
		public static void GetBan(string sClientSteamID, CCSPlayerController? admin, string reason, bool bConsole, EbanDB.GetBanCommFunc handler)
#nullable disable
		{
			EbanDB.GetBan(sClientSteamID, EW.g_Scheme.server_name, admin, reason, bConsole, handler);
		}
		public static void GetBan(string sClientSteamID, EbanDB.GetBanAPIFunc handler)
		{
			EbanDB.GetBan(sClientSteamID, EW.g_Scheme.server_name, handler);
		}


		public EbanPlayer()
        {
            bBanned = false;
            bFixSpawnItem = false;
		}
    }
}
