using CounterStrikeSharp.API.Core;
using EntWatchSharp.Items;
using EntWatchSharp.Modules.Eban;
using EntWatchSharpAPI;

namespace EntWatchSharp
{
	internal class EWAPI : IEntWatchSharpAPI
	{
		public async Task<SEWAPI_Ban> Native_EntWatch_IsClientBanned(string sSteamID)
		{
			if (!string.IsNullOrEmpty(sSteamID))
			{
				EbanPlayer eban = await EbanDB.GetBan(sSteamID, EW.g_Scheme.server_name);
				if (eban != null)
				{
					SEWAPI_Ban apieban = new SEWAPI_Ban();
					apieban.bBanned = eban.bBanned;
					apieban.sAdminName = eban.sAdminName;
					apieban.sAdminSteamID = eban.sAdminSteamID;
					apieban.iDuration = eban.iDuration;
					apieban.iTimeStamp_Issued = eban.iTimeStamp_Issued;
					apieban.sReason = eban.sReason;
					apieban.sClientName = eban.sClientName;
					apieban.sClientSteamID = eban.sClientSteamID;
					return apieban;
				}
			}
			return new SEWAPI_Ban();
		}
		public async Task<bool> Native_EntWatch_BanClient(SEWAPI_Ban sewPlayer)
		{
			return await EbanDB.BanClient(sewPlayer.sClientName, sewPlayer.sClientSteamID, sewPlayer.sAdminName, sewPlayer.sAdminSteamID, EW.g_Scheme.server_name, sewPlayer.iDuration, sewPlayer.iTimeStamp_Issued, sewPlayer.sReason);
		}
		public async Task<bool> Native_EntWatch_UnbanClient(SEWAPI_Ban sewPlayer)
		{
			return await EbanDB.UnBanClient(sewPlayer.sClientSteamID, sewPlayer.sAdminName, sewPlayer.sAdminSteamID, EW.g_Scheme.server_name, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), sewPlayer.sReason);
		}
		public async Task Native_EntWatch_UpdateStatusBanClient(CCSPlayerController Player)
		{
			if (!await EW.g_BannedPlayer[Player].GetBan(Player)) EW.g_BannedPlayer[Player].bBanned = false;
		}
		public bool Native_EntWatch_IsSpecialItem(CEntityInstance cEntity)
		{
			if (!cEntity.IsValid) return false;
			foreach (Item ItemTest in EW.g_ItemList.ToList())
			{
				if (ItemTest.WeaponHandle == cEntity) return true;
			}
			return false;
		}
		public bool Native_EntWatch_IsButtonSpecialItem(CEntityInstance cEntity)
		{
			if (!cEntity.IsValid) return false;
			foreach (Item ItemTest in EW.g_ItemList.ToList())
			{
				foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
				{
					if (AbilityTest.Entity == cEntity) return true;
				}
			}
			return false;
		}
		public bool Native_EntWatch_HasSpecialItem(CCSPlayerController player)
		{
			if (!player.IsValid) return false;
			foreach (Item ItemTest in EW.g_ItemList.ToList())
			{
				if (ItemTest.Owner == player) return true;
			}
			return false;
		}
		//===================================================================================================
		public event IEntWatchSharpAPI.Forward_OnClientBanned Forward_EntWatch_OnClientBanned;
		public void OnClientBanned(SEWAPI_Ban sewPlayer) => Forward_EntWatch_OnClientBanned?.Invoke(sewPlayer);
		//===================================================================================================
		public event IEntWatchSharpAPI.Forward_OnClientUnbanned Forward_EntWatch_OnClientUnbanned;
		public void OnClientUnbanned(SEWAPI_Ban sewPlayer) => Forward_EntWatch_OnClientUnbanned?.Invoke(sewPlayer);
		//===================================================================================================
		public event IEntWatchSharpAPI.Forward_OnUseItem Forward_EntWatch_OnUseItem;
		public void OnUseItem(string sItemName, CCSPlayerController Player, string sAbility) => Forward_EntWatch_OnUseItem?.Invoke(sItemName, Player, sAbility);
		//===================================================================================================
		public event IEntWatchSharpAPI.Forward_OnPickUpItem Forward_EntWatch_OnPickUpItem;
		public void OnPickUpItem(string sItemName, CCSPlayerController Player) => Forward_EntWatch_OnPickUpItem?.Invoke(sItemName, Player);
		//===================================================================================================
		public event IEntWatchSharpAPI.Forward_OnDropItem Forward_EntWatch_OnDropItem;
		public void OnDropItem(string sItemName, CCSPlayerController Player) => Forward_EntWatch_OnDropItem?.Invoke(sItemName, Player);
		//===================================================================================================
		public event IEntWatchSharpAPI.Forward_OnPlayerDisconnectWithItem Forward_EntWatch_OnPlayerDisconnectWithItem;
		public void OnPlayerDisconnectWithItem(string sItemName, CCSPlayerController Player) => Forward_EntWatch_OnPlayerDisconnectWithItem?.Invoke(sItemName, Player);
		//===================================================================================================
		public event IEntWatchSharpAPI.Forward_OnPlayerDeathWithItem Forward_EntWatch_OnPlayerDeathWithItem;
		public void OnPlayerDeathWithItem(string sItemName, CCSPlayerController Player) => Forward_EntWatch_OnPlayerDeathWithItem?.Invoke(sItemName, Player);
		//===================================================================================================
		public event IEntWatchSharpAPI.Forward_OnAdminSpawnItem Forward_EntWatch_OnAdminSpawnItem;
		public void OnAdminSpawnItem(CCSPlayerController Admin, string sItemName, CCSPlayerController Target) => Forward_EntWatch_OnAdminSpawnItem?.Invoke(Admin, sItemName, Target);
		//===================================================================================================
		public event IEntWatchSharpAPI.Forward_OnAdminTransferedItem Forward_EntWatch_OnAdminTransferedItem;
		public void OnAdminTransferedItem(CCSPlayerController Admin, string sItemName, CCSPlayerController Receiver) => Forward_EntWatch_OnAdminTransferedItem?.Invoke(Admin, sItemName, Receiver);
		//===================================================================================================
	}
}
