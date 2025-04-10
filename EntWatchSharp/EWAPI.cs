using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using EntWatchSharp.Items;
using EntWatchSharp.Modules.Eban;
using EntWatchSharpAPI;

namespace EntWatchSharp
{
	internal class EWAPI : IEntWatchSharpAPI
	{
		public void Native_EntWatch_IsClientBanned(string sSteamID)
		{
			if (!string.IsNullOrEmpty(sSteamID)) EbanPlayer.GetBan(sSteamID, GetBanAPI_Handler);
			/*{
				EbanPlayer eban = EbanDB.GetBan(sSteamID, EW.g_Scheme.server_name);
				if (eban != null)
				{
					SEWAPI_Ban apieban = new()
					{
						bBanned = eban.bBanned,
						sAdminName = eban.sAdminName,
						sAdminSteamID = eban.sAdminSteamID,
						iDuration = eban.iDuration,
						iTimeStamp_Issued = eban.iTimeStamp_Issued,
						sReason = eban.sReason,
						sClientName = eban.sClientName,
						sClientSteamID = eban.sClientSteamID
					};
					return apieban;
				}
			}
			return new SEWAPI_Ban();*/
		}
		EbanDB.GetBanAPIFunc GetBanAPI_Handler = (string sClientSteamID, List<List<string>> DBQuery_Result) =>
		{
			if (DBQuery_Result.Count > 0)
			{
				SEWAPI_Ban target = new()
				{
					bBanned = true,
					sAdminName = DBQuery_Result[0][0],
					sAdminSteamID = DBQuery_Result[0][1],
					iDuration = Convert.ToInt32(DBQuery_Result[0][2]),
					iTimeStamp_Issued = Convert.ToInt32(DBQuery_Result[0][3]),
					sReason = DBQuery_Result[0][4],
					sClientName = DBQuery_Result[0][5],
					sClientSteamID = sClientSteamID
				};
				EW.g_cAPI?.IsClientBannedResult(target);
				return;
			}
			EW.g_cAPI?.IsClientBannedResult(new SEWAPI_Ban());
		};
		public void Native_EntWatch_BanClient(SEWAPI_Ban sewPlayer)
		{
			EbanDB.BanClient(sewPlayer.sClientName, sewPlayer.sClientSteamID, sewPlayer.sAdminName, sewPlayer.sAdminSteamID, EW.g_Scheme.server_name, sewPlayer.iDuration, sewPlayer.iTimeStamp_Issued, sewPlayer.sReason);
		}
		public void Native_EntWatch_UnbanClient(SEWAPI_Ban sewPlayer)
		{
			EbanDB.UnBanClient(sewPlayer.sClientSteamID, sewPlayer.sAdminName, sewPlayer.sAdminSteamID, EW.g_Scheme.server_name, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), sewPlayer.sReason);
		}
		public void Native_EntWatch_UpdateStatusBanClient(CCSPlayerController Player)
		{
			EbanPlayer.GetBan(Player);
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
		public void Native_EntWatch_EnableWeaponGlow(CCSPlayerController player)
		{
			if(player.IsValid && EW.CheckDictionary(player)) EW.g_EWPlayer[player].PrivilegePlayer.WeaponGlow = true;
		}
		public void Native_EntWatch_DisableWeaponGlow(CCSPlayerController player)
		{
			if (player.IsValid && EW.CheckDictionary(player)) EW.g_EWPlayer[player].PrivilegePlayer.WeaponGlow = false;
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
		public event IEntWatchSharpAPI.Forward_IsClientBannedResult Forward_EntWatch_IsClientBannedResult;
		public void IsClientBannedResult(SEWAPI_Ban sewPlayer) => Forward_EntWatch_IsClientBannedResult?.Invoke(sewPlayer);
		//===================================================================================================
	}
}
