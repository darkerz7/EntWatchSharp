# [Core]EntWatchSharp for CounterStrikeSharp
Notify players about entity interactions
Alpha version of the plugin, needs many improvements

## Features:
1. JSON Configs
2. Multi-button control
3. Working with game_ui(Logic_case)
4. Block item pick up on E
5. It is possible to transfer a discarded or not yet selected item
6. You can specify the reason for the eban
7. Allows you to use the item in the crowd
8. Information output in HUD, which can be configured by each player separately
9. Allows you to track {number} buttons
10. Async functions
11. SQLite and MySQL support
12. Language setting for players
13. Allows you to set up individual access for admins
14. Keeps logs to a file and discord
15. The database is compatible with the old [EntWatch_DZ](https://github.com/darkerz7/EntWatch_DZ)

## Required packages:
1. [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/)
2. [MySqlConnector](https://www.nuget.org/packages/MySqlConnector/2.3.7?_src=template) (2.3.7)
3. [System.Data.SQLite.Core](https://www.nuget.org/packages/System.Data.SQLite.Core/1.0.117?_src=template) (1.0.117 only; 1.0.118 don't work)
4. [CS2-HammerIDFix](https://github.com/darkerz7/CS2-HammerIDFix)
5. [ClientPrefs_CS2](https://github.com/darkerz7/ClientPrefs_CS2)
6. Recomended [CS2-EntityFix](https://github.com/darkerz7/CS2-EntityFix)

## Installation:
1. Install `CS2-HammerIDFix`, `ClientPrefs_CS2` and `CS2-EntityFix`
2. Compile or copy EntWatchSharp to `counterstrikesharp/plugins/EntWatchSharp` folger
3. Copy and configure the configuration file `db_config.json` and `log_config.json` to `counterstrikesharp/plugins/EntWatchSharp` folger
4. Install or copy DLL from `Required packages` (`MySqlConnector.dll`, `SQLite.Interop.dll`, `System.Data.SQLite.dll`) to counterstrikesharp/plugins/EntWatchSharp folger
5. Copy `lang` folger to `counterstrikesharp/plugins/EntWatchSharp/lang` folger
6. Copy `gamedata` to `counterstrikesharp/plugins/EntWatchSharp/gamedata` folger 
7. Copy and configure `mapsconfig` and `schemes` to `addons/entwatch` folger
8. Add CVARs to server.cfg
9. Restart server

## Example MapConfig
```
[
	{
		"Name": "",						//String, FullName of Item (Chat)
		"ShortName": "",				//String, ShortName of Item (Hud)
		"Color": "",					//String, One of the colors. (Chat)
		"HammerID": 0,					//Integer, weapon_* HammerID
		"GlowColor": [0,0,0,0],			//Array[4], One of the colors. (Glow)
		"FilterID": 0,					//Integer, not in use yet
		"FilterValue": "",				//String, not in use yet
		"BlockPickup": false,			//Bool, The item cannot be picked up
		"AllowTransfer": false,			//Bool, Allow admins to transfer an item
		"ForceDrop": false,				//Bool, The item will be dropped if player dies or disconnects
		"Chat": false,					//Bool, Display chat items
		"Hud": false,					//Bool, Display Hud items
		"TriggerID": 0,					//Integer, Sets a trigger that an ebanned player cant activate, mostly to prevent picking up weapon_knife items
		"UsePriority": false,			//Bool, Enabled by default. You can disable the forced pressing of the button on a specific item
		"AbilityList": [				//Array of abilities
			{
				"Name": "",				//String, Custom ability name, can be omitted
				"ButtonID": 0,			//Integer, Allows you to sort buttons
				"ButtonClass": "",		//String, Button Class, Can use "game_ui" for anoter activation method
				"Chat_Uses": false,		//Bool, Display chat someone is using an item(if disabled chat)
				"Mode": 0,				//Integer, Mode for item. 0 = Can hold E, 1 = Spam protection only, 2 = Cooldowns, 3 = Limited uses, 4 = Limited uses with cooldowns, 5 = Cooldowns after multiple uses
				"MaxUses": 0,			//Integer, Maximum uses for modes 3, 4, 5
				"CoolDown": 0			//Integer, Cooldown of item for modes 2, 4, 5
			},
			{
				"Name": "",
				"ButtonID": 0,
				"ButtonClass": "game_ui::PressedAttack",	//Example for Game_UI
				"Chat_Uses": false,
				"Mode": 0,
				"MaxUses": 0,
				"CoolDown": 0
			}
		]
	},
	{
		"Name": <Next Item...>
	}
]
```

## Admin privileges
Privilege | Description
--- | ---
`@css/ew_reload` | Allows you to reload the plugin settings and view them
`@css/ew_chat` | Allows you to view messages about item selection in team mode
`@css/ew_hud` | Allows you to display items in command mode
`@css/ew_ban` | Allows access to bans (Command)
`@css/ew_ban_perm` | Allows access to permanent bans (Duration 0)
`@css/ew_ban_long` | Allows access to long bans (Cvar ewc_banlong)
`@css/ew_unban` | Allows access to unbans (Command)
`@css/ew_unban_perm` | Allows access to permanent unbans (Duration 0)
`@css/ew_unban_other` | Allows access to unbans from other admins
`@css/ew_transfer` | Allows transfer of items

## CVARs
Cvar | Parameters | Description
--- | --- | ---
`ewc_teamonly` | `<false-true>` | Enable/Disable team only mode. (Default true)
`ewc_adminchat` | `<0-2>` | Change Admin Chat Mode (0 - All Messages, 1 - Only Pickup/Drop Items, 2 - Nothing). (Default 0)
`ewc_adminhud` | `<0-2>` | Change Admin Hud Mode (0 - All Items, 1 - Only Item Name, 2 - Nothing). (Default 0)
`ewc_blockepick` | `<false-true>` | Block players from using E key to grab items. (Default true)
`ewc_delay_use` | `<0.0-60.0>` | Change delay before use. (Default 1.0)
`ewc_globalblock` | `<false-true>` | Blocks the pickup of any items by players. (Default false)
`ewc_display_ability` | `<0-4>` | Count of the abilities to display on the HUD. (Default 4)
`ewc_use_priority` | `<false-true>` | Enable/Disable forced pressing of the button. (Default true)
`ewc_path_scheme` | `<string>` | Path with filename for the scheme. (Default addons/entwatch/scheme/default.json)
`ewc_path_cfg` | `<string>` | Directory for configs. (Default addons/entwatch/maps/)
`ewc_triggeronce` | `<false-true>` | Exclude trigger_once from ban check. (Default true)
`ewc_glow_spawn` | `<false-true> `| Enable/Disable the glow after Spawn Items. (Default true)
`ewc_glow_particle` | `<false-true>` | Enable/Disable the glow using a particle. (Default true)
`ewc_bantime` | `<0-43200>` | Default ban time. 0 - Permanent. (Default 0)
`ewc_banlong` | `<1-1440000>` | Max ban time with once @css/ew_ban privilege. (Default 720)
`ewc_banreason` | `<string>` | Default ban reason. (Default Trolling)
`ewc_unbanreason` | `<string>` | Default unban reason. (Default Giving another chance)
`ewc_keep_expired_ban` | `<false-true>` | Enable/Disable keep expired bans. (Default true)

## Commands
Client Command | Description
--- | ---
`ehud` | Allows the player to switch the HUD (0 - Disabled, 1 - Center, 2 - Alert, 3 - WorldText)
`ehud_pos` | Allows the player to change the position of the HUD {X Y Z} (default: 50 50 50; min -200.0; max 200.0)
`ehud_refresh` | Allows the player to change the time it takes to scroll through the list {sec} (default: 3; min 1; max 10)
`ehud_sheet` | Allows the player to change the number of items on the sheet {count} (default: 5; min 1; max 15)
`eup` | Allows the player to use UsePriority {bool}
`ew_status` | Allows the player to view the restrictions {null/target}

## Admin's commands
Admin Command | Privilege | Description
--- | --- | ---
`ew_reload` | `@css/ew_reload` | Reloads config and Scheme
`ew_showitems` | `@css/ew_reload` | Shows a list of spawned items
`ew_showscheme` | `@css/ew_reload` | Shows the scheme
`ew_ban` | `@css/ew_ban`+`@css/ew_ban_perm`+`@css/ew_ban_long` | Allows the admin to restrict items for the player `<#userid/name> [<time>] [<reason>]`
`ew_unban` | `@css/ew_unban`+`@css/ew_unban_perm`+`@css/ew_unban_other` | Allows the admin to remove the item restriction for a player `<#userid/name> [<reason>]`
`ew_banlist` | `@css/ew_ban` | Displays a list of restrictions
`ew_transfer` | `@css/ew_transfer` | Allows the admin to transfer items `<owner>/$<itemname> <receiver>`

## Future plans
1. Fixes Errors
2. Add mods 6 and 7 for counters
3. Add filter processing
4. Add display to clan tag
5. Add commands to change items
6. Add ban offline players
7. Add item spawn