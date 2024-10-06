using CounterStrikeSharp.API.Core;

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

        public async Task<bool> SetBan(string sBanAdminName, string sBanAdminSteamID, string sBanClientName, string sBanClientSteamID, int iBanDuration, string sBanReason)
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
                return await EbanDB.BanClient(sBanClientName, sBanClientSteamID, sAdminName, sAdminSteamID, EW.g_Scheme.server_name, iDuration, iTimeStamp_Issued, sReason);
            }
            return false;
        }

        public async Task<bool> UnBan(string sUnBanAdminName, string sUnBanAdminSteamID, string sUnBanClientSteamID, string sUnbanReason)
        {
            if (!string.IsNullOrEmpty(sUnBanClientSteamID))
            {
                bBanned = false;
                if (string.IsNullOrEmpty(sUnbanReason)) sUnbanReason = "Amnesty";
                return await EbanDB.UnBanClient(sUnBanClientSteamID, sUnBanAdminName, sUnBanAdminSteamID, EW.g_Scheme.server_name, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), sUnbanReason);
            }
            return false;
        }

        public async Task<bool> GetBan(CCSPlayerController player)
        {
            if (player.IsValid)
            {
                return await EbanDB.GetBan(player, EW.g_Scheme.server_name);
            }
            else
            if (EW.g_BannedPlayer.ContainsKey(player))
                EW.g_BannedPlayer.Remove(player);
            return false;
        }

        public EbanPlayer()
        {
            bBanned = false;
            bFixSpawnItem = false;
		}
    }
}
