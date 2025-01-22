using CounterStrikeSharp.API.Core;
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
                    return true;
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
                return EbanDB.BanClient(sBanClientName, sBanClientSteamID, sAdminName, sAdminSteamID, EW.g_Scheme.server_name, iDuration, iTimeStamp_Issued, sReason);
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
				return EbanDB.UnBanClient(sUnBanClientSteamID, sUnBanAdminName, sUnBanAdminSteamID, EW.g_Scheme.server_name, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), sUnbanReason);
            }
            return false;
        }

        public static bool GetBan(CCSPlayerController player)
        {
            if (player.IsValid)
            {
                return EbanDB.GetBan(player, EW.g_Scheme.server_name);
            }
            return false;
        }

        public EbanPlayer()
        {
            bBanned = false;
            bFixSpawnItem = false;
		}
    }
}
