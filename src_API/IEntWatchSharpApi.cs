using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;

//VersionAPI: 0.DZ.2

namespace EntWatchSharpAPI
{
	public struct SEWAPI_Ban
	{
		public bool bBanned;                //True if user is banned, false otherwise

		public string sAdminName;			//Nickname admin who issued the ban
		public string sAdminSteamID;		//SteamID admin who issued the ban
		public int iDuration;               //Duration of the ban -1 - Temporary, 0 - Permamently, Positive value - time in minutes
		public int iTimeStamp_Issued;		//Pass an integer variable by reference and it will contain the UNIX timestamp when the player will be unbanned/ when a player was banned if ban = Permamently/Temporary
		public string sReason;				//The reason why the player was banned

		public string sClientName;			//Nickname of the player who got banned
		public string sClientSteamID;		//SteamID of the player who got banned

		public SEWAPI_Ban()
		{
			bBanned = false;
			sAdminName = "Console";
			sAdminSteamID = "SERVER";
			iDuration = 0;
			iTimeStamp_Issued = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			sReason = "No Reason";
			sClientName = "";
			sClientSteamID = "";
		}
	}
	public interface IEntWatchSharpAPI
	{
		public static PluginCapability<IEntWatchSharpAPI> Capability { get; } = new("entwatch:api");

		/**
		 * Checks if a player is currently banned, if an integer variable is referenced the time of unban will be assigned to it.
		 *
		 * @param sSteamID		SteamID of the player to check for ban
		 * @return				SEWAPI_Ban struct
		 *
		 */
		Task<SEWAPI_Ban> Native_EntWatch_IsClientBanned(string sSteamID);

		/**
		 * Bans a player from using special items.
		 *
		 * @param sewPlayer		SEWAPI_Ban struct to ban
		 * @return				True on success, false otherwsie
		 *
		 * On error/errors:		Invalid player
		 */
		Task<bool> Native_EntWatch_BanClient(SEWAPI_Ban sewPlayer);

		/**
		 * Unbans a previously ebanned player.
		 *
		 * @param sewPlayer		SEWAPI_Ban struct to unban
		 * @return				True on success, false otherwsie
		 *
		 * On error/errors:		Invalid player
		 */
		Task<bool> Native_EntWatch_UnbanClient(SEWAPI_Ban sewPlayer);

		/**
		 * Forces a ban status update.
		 *
		 * @param Player		CCSPlayerController for forced update
		 *
		 * On error/errors:		Invalid player
		 */
		Task Native_EntWatch_UpdateStatusBanClient(CCSPlayerController Player);

		/**
		 * Checks if an entity is a special item.
		 *
		 * @param cEntity		CEntityInstance to check
		 * @return				True if entity is a special item, false otherwsie
		 */
		bool Native_EntWatch_IsSpecialItem(CEntityInstance cEntity);

		/**
		 * Checks if an entity is a special item button.
		 *
		 * @param cEntity		CEntityInstance to check
		 * @return				True if entity is a special item, false otherwsie
		 */
		bool Native_EntWatch_IsButtonSpecialItem(CEntityInstance cEntity);

		/**
		 * Checks if a player has a special item.
		 *
		 * @param Player		Player to check
		 * @return				True if player has a special item, false otherwsie
		 */
		bool Native_EntWatch_HasSpecialItem(CCSPlayerController Player);

		/**
		 * Called when a player is e-banned by any means
		 *
		 * @param sewPlayer		Full information about ban in SEWAPI_Ban struct
		 *
		 * @return				None
		 */
		public delegate void Forward_OnClientBanned(SEWAPI_Ban sewPlayer);
		public event Forward_OnClientBanned Forward_EntWatch_OnClientBanned;

		/**
		 * Called when a player is e-unbanned by any means
		 *
		 * @param sewPlayer		Full information about unban in SEWAPI_Ban struct
		 * @return				None
		 */
		public delegate void Forward_OnClientUnbanned(SEWAPI_Ban sewPlayer);
		public event Forward_OnClientUnbanned Forward_EntWatch_OnClientUnbanned;

		/**
		 * Сalled when a player is use item
		 *
		 * @param sItemName		The name of the item that was used
		 * @param Player		CCSPlayerController that was used item
		 * @param sAbility		The ability name of the item that was used
		 * @return				None
		 */
		public delegate void Forward_OnUseItem(string sItemName, CCSPlayerController Player, string sAbility);
		public event Forward_OnUseItem Forward_EntWatch_OnUseItem;

		/**
		 * Сalled when a player is pickup item
		 *
		 * @param sItemName		The name of the item that was picked
		 * @param Player		CCSPlayerController that was picked item
		 * @return				None
		 */
		public delegate void Forward_OnPickUpItem(string sItemName, CCSPlayerController Player);
		public event Forward_OnPickUpItem Forward_EntWatch_OnPickUpItem;

		/**
		 * Сalled when a player is drop item
		 *
		 * @param sItemName		The name of the item that was dropped
		 * @param Player		CCSPlayerController that was dropped item
		 * @return				None
		 */
		public delegate void Forward_OnDropItem(string sItemName, CCSPlayerController Player);
		public event Forward_OnDropItem Forward_EntWatch_OnDropItem;

		/**
		 * Сalled when a player is disconnect with item
		 *
		 * @param sItemName		The name of the item that the disconnected player had
		 * @param Player		CCSPlayerController that was disconnected with item
		 * @return				None
		 */
		public delegate void Forward_OnPlayerDisconnectWithItem(string sItemName, CCSPlayerController Player);
		public event Forward_OnPlayerDisconnectWithItem Forward_EntWatch_OnPlayerDisconnectWithItem;

		/**
		 * Сalled when a player is death with item
		 *
		 * @param sItemName		The name of the item that player had when death
		 * @param Player		CCSPlayerController that was death with item
		 * @return				None
		 */
		public delegate void Forward_OnPlayerDeathWithItem(string sItemName, CCSPlayerController Player);
		public event Forward_OnPlayerDeathWithItem Forward_EntWatch_OnPlayerDeathWithItem;

		/**
		 * Called when a admin was spawned item
		 *
		 * @param Admin			Admin CCSPlayerController that was spawned item
		 * @param sItemName		The name of the item that was spawned
		 * @param Target		Target CCSPlayerController that received the item
		 *
		 * @return				None
		 */
		public delegate void Forward_OnAdminSpawnItem(CCSPlayerController Admin, string sItemName, CCSPlayerController Target);
		public event Forward_OnAdminSpawnItem Forward_EntWatch_OnAdminSpawnItem;

		/**
		 * Called when a admin was transfered item to receiver
		 *
		 * @param Admin		Admin CCSPlayerController that was transfered item
		 * @param sItemName		The name of the item transfered to the receiver
		 * @param Receiver		Receiver CCSPlayerController that received the item
		 *
		 * @return				None
		 */
		public delegate void Forward_OnAdminTransferedItem(CCSPlayerController Admin, string sItemName, CCSPlayerController Target);
		public event Forward_OnAdminTransferedItem Forward_EntWatch_OnAdminTransferedItem;
	}
}
