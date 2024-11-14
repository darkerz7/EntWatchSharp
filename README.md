# [Core]EntWatchSharp for CounterStrikeSharp
Notify players about entity interactions
Beta version of the plugin, needs many improvements

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
16. Work with math_counter (Mode: 6, 7) and HP (Mode: 8) is available
17. In-game config change
18. Offline ew_ban/ew_unban
19. Applying filters for the activator
20. Items spawn
21. API for interaction with other plugins
22. Transmit HUD for other players

## Required packages:
1. [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/) (Min version: 285)
2. [MySqlConnector](https://www.nuget.org/packages/MySqlConnector/2.4.0?_src=template) (2.4.0)
3. [System.Data.SQLite.Core](https://www.nuget.org/packages/System.Data.SQLite.Core/1.0.119?_src=template) (1.0.119)
4. [CS2-HammerIDFix](https://github.com/darkerz7/CS2-HammerIDFix)
5. [ClientPrefs_CS2](https://github.com/darkerz7/ClientPrefs_CS2)
6. Recomended [CS2-EntityFix](https://github.com/darkerz7/CS2-EntityFix)
7. Recomended [CS2-CustomIO](https://github.com/darkerz7/CS2-CustomIO)
8. Recomended [CSSharp-Fixes](https://github.com/darkerz7/CSSharp-Fixes)

## Installation:
1. Install `CS2-HammerIDFix`, `ClientPrefs_CS2`, `CS2-CustomIO`, `CS2-EntityFix` and `CSSharp-Fixes`
2. Compile or copy EntWatchSharp to `counterstrikesharp/plugins/EntWatchSharp` folger
3. Copy and configure the configuration file `db_config.json` and `log_config.json` to `counterstrikesharp/plugins/EntWatchSharp` folger
4. Install or copy DLL from `Required packages` (`MySqlConnector.dll`, `SQLite.Interop.dll`, `System.Data.SQLite.dll`) to counterstrikesharp/plugins/EntWatchSharp folger
5. Copy `lang` folger to `counterstrikesharp/plugins/EntWatchSharp/lang` folger
6. Copy `gamedata` to `counterstrikesharp/plugins/EntWatchSharp/gamedata` folger 
7. Copy and configure `mapsconfig` and `schemes` to `addons/entwatch` folger
8. Compile or copy EntWatchSharpAPI to `counterstrikesharp/shared/EntWatchSharpAPI` folger
9. Add CVARs to server.cfg
10. Restart server

## Example MapConfig
```
[
	{
		"Name": "",					//String, FullName of Item (Chat)
		"ShortName": "",				//String, ShortName of Item (Hud)
		"Color": "",					//String, One of the colors. (Chat)
		"HammerID": 0,					//Integer, weapon_* HammerID
		"GlowColor": [0,0,0,0],				//Array[4], One of the colors. (Glow)
		"BlockPickup": false,				//Bool, The item cannot be picked up
		"AllowTransfer": false,				//Bool, Allow admins to transfer an item
		"ForceDrop": false,				//Bool, The item will be dropped if player dies or disconnects
		"Chat": false,					//Bool, Display chat items
		"Hud": false,					//Bool, Display Hud items
		"TriggerID": 0,					//Integer, Sets a trigger that an ebanned player cant activate, mostly to prevent picking up weapon_knife items
		"UsePriority": false,				//Bool, Enabled by default. You can disable the forced pressing of the button on a specific item
		"SpawnerID": 0,					//Integer, Allows admins to spawn items. Not recommended to use because it can break the items. Type point_template's HammerID which spawns the item
		"AbilityList": [				//Array of abilities
			{
				"Name": "",			//String, Custom ability name, can be omitted
				"ButtonID": 0,			//Integer, Allows you to sort buttons
				"ButtonClass": "",		//String, Button Class, Can use "game_ui" for anoter activation method
				"Filter": "",			//String, Filter value for activator. |$attribute| for filter_activator_attribute_int (starts with $); |context:value| for filter_activator_context (contains :); other for filter_activator_name
				"Chat_Uses": false,		//Bool, Display chat someone is using an item(if disabled chat)
				"Mode": 0,			//Integer, Mode for item. 0 = Can hold E, 1 = Spam protection only, 2 = Cooldowns, 3 = Limited uses, 4 = Limited uses with cooldowns, 5 = Cooldowns after multiple uses, 6 = Counter - stops when minimum is reached, 7 = Counter - stops when maximum is reached, 8 = Health button
				"MaxUses": 0,			//Integer, Maximum uses for modes 3, 4, 5
				"CoolDown": 0,			//Integer, Cooldown of item for modes 2, 4, 5
				"Ignore": false,		//Bool, Ignore item display
				"LockItem": false,		//Bool, Lock button/door/game_ui_IO
				"MathID": 0,			//Integer, math_counter HammerID for modes 6, 7
				"MathNameFix": false		//Bool, Fix the name of the math_counter (Work with flag: Preserve entity names (Don't do name fixup) ->point_template/env_entity_maker)
			},
			{
				"Name": "",
				"ButtonID": 0,
				"ButtonClass": "game_ui::PressedAttack",	//Example for Game_UI
				"Filter": "",
				"Chat_Uses": false,
				"Mode": 0,
				"MaxUses": 0,
				"CoolDown": 0,
				"Ignore": false,
				"LockItem": false,
				"MathID": 0,
				"MathNameFix": false
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
`@css/ew_map` | Allows you to change configs in the game
`@css/ew_spawn` | Allows items to spawn

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
`ewc_display_mapcommands` | `<false-true>` | Enable/Disable display of item changes. (Default true)
`ewc_path_scheme` | `<string>` | Path with filename for the scheme. (Default addons/entwatch/scheme/default.json)
`ewc_path_cfg` | `<string>` | Directory for configs. (Default addons/entwatch/maps/)
`ewc_lower_mapname` | `<false-true>` | Automatically lowercase map name. (Default false)
`ewc_triggeronce` | `<false-true>` | Exclude trigger_once from ban check. (Default true)
`ewc_glow_spawn` | `<false-true> `| Enable/Disable the glow after Spawn Items. (Default true)
`ewc_glow_particle` | `<false-true>` | Enable/Disable the glow using a particle. (Default true)
`ewc_bantime` | `<0-43200>` | Default ban time. 0 - Permanent. (Default 0)
`ewc_banlong` | `<1-1440000>` | Max ban time with once @css/ew_ban privilege. (Default 720)
`ewc_banreason` | `<string>` | Default ban reason. (Default Trolling)
`ewc_unbanreason` | `<string>` | Default unban reason. (Default Giving another chance)
`ewc_keep_expired_ban` | `<false-true>` | Enable/Disable keep expired bans. (Default true)
`ewc_offline_clear_time` | `<1-240>` | Time during which data is stored. (Default 30)

## Commands
Client Command | Description
--- | ---
`ehud` | Allows the player to switch the HUD (0 - Disabled, 1 - Center, 2 - Alert, 3 - WorldText)
`ehud_pos` | Allows the player to change the position of the HUD {X Y Z} (default: 50 50 50; min -200.0; max 200.0)
`ehud_refresh` | Allows the player to change the time it takes to scroll through the list {sec} (default: 3; min 1; max 10)
`ehud_sheet` | Allows the player to change the number of items on the sheet {count} (default: 5; min 1; max 15)
`eup` | Allows the player to use UsePriority {bool}
`ew_status`<br>`css_estatus` | Allows the player to view the restrictions {null/target}

## Admin's commands
Admin Command | Privilege | Description
--- | --- | ---
`ew_reload`<br>`css_ereload` | `@css/ew_reload` | Reloads config and Scheme
`ew_showitems`<br>`css_eshowitems` | `@css/ew_reload` | Shows a list of spawned items
`ew_showscheme`<br>`css_eshowscheme` | `@css/ew_reload` | Shows the scheme
`ew_ban`<br>`css_eban` | `@css/ew_ban`+`@css/ew_ban_perm`+`@css/ew_ban_long` | Allows the admin to restrict items for the player `<#userid/name> [<time>] [<reason>]`
`ew_unban`<br>`css_eunban` | `@css/ew_unban`+`@css/ew_unban_perm`+`@css/ew_unban_other` | Allows the admin to remove the item restriction for a player `<#userid/name> [<reason>]`
`ew_banlist`<br>`css_ebanlist` | `@css/ew_ban` | Displays a list of restrictions
`ew_transfer`<br>`css_etransfer` | `@css/ew_transfer` | Allows the admin to transfer items `<owner>/$<itemname> <receiver>`
`ew_spawn`<br>`css_espawn` | `@css/ew_spawn` | Allows the admin to spawn items `<itemname> <receiver> [<strip>]`
`ew_list`<br>`css_elist` | `@css/ew_ban` | Shows a list of players including those who have disconnected

## Mapper's Commands
Map Command | Variables | Description
--- | --- | ---
`ew_setcooldown` | `<int hammerid> <int buttonid> <int new cooldown> [<bool force apply>]` | Allows you to change the item’s cooldown during the game
`ew_setmaxuses` | `<int hammerid> <int buttonid> <int maxuses> [<bool even if over>]` | Allows you to change the maximum use of the item during the game, depending on whether the item was used to the end
`ew_setuses` | `<int hammerid> <int buttonid> <int value> [<bool even if over>]` | Allows you to change the current use of the item during the game, depending on whether the item was used to the end
`ew_addmaxuses` | `<int hammerid> <int buttonid> [<bool even if over>]` | Allows you to add 1 charge to the item, depending on whether the item was used to the end
`ew_setmode` | `<int hammerid> <int buttonid> <int newmode> <int cooldown> <int maxuses> [<bool even if over>]` | Allows you to completely change the item
`ew_lockbutton` | `<int hammerid> <int buttonid> <bool value>` | Allows to lock item
`ew_setabilityname` | `<int hammerid> <int buttonid> <string newname>` | Allows you to change the ability’s name
`ew_setname` | `<int hammerid> <string newname>` | Allows you to change the item’s name(Chat)
`ew_setshortname` | `<int hammerid> <string newshortname>` | Allows you to change the item’s shortname(HUD)
`ew_block` | `<int hammerid> <bool value>` | Allows you to block an item during the game. Similar to the 'blockpickup' property

PS:<br>
...The values ​​of the int must be greater than or equal to 0<br>
...Mode values ​​must be between 0 and 8<br>
...String values ​​must not be empty/null<br>
...To work with these commands there must be a flag: `@css/ew_map`

## Example of SQL-request for correct use of STEAMID after CS:GO/EntWatchSharp below version 0.DZ.7.beta
```
UPDATE entwatch_current_eban
SET client_steamid = REPLACE(client_steamid, 'STEAM_1:', 'STEAM_0:')
WHERE client_steamid LIKE '%STEAM_1:%';

UPDATE entwatch_current_eban
SET admin_steamid = REPLACE(admin_steamid, 'STEAM_1:', 'STEAM_0:')
WHERE admin_steamid LIKE '%STEAM_1:%';

UPDATE entwatch_current_eban
SET admin_steamid_unban = REPLACE(admin_steamid_unban, 'STEAM_1:', 'STEAM_0:')
WHERE admin_steamid_unban LIKE '%STEAM_1:%';

UPDATE entwatch_old_eban
SET client_steamid = REPLACE(client_steamid, 'STEAM_1:', 'STEAM_0:')
WHERE client_steamid LIKE '%STEAM_1:%';

UPDATE entwatch_old_eban
SET admin_steamid = REPLACE(admin_steamid, 'STEAM_1:', 'STEAM_0:')
WHERE admin_steamid LIKE '%STEAM_1:%';

UPDATE entwatch_old_eban
SET admin_steamid_unban = REPLACE(admin_steamid_unban, 'STEAM_1:', 'STEAM_0:')
WHERE admin_steamid_unban LIKE '%STEAM_1:%';
```

## Future plans
1. Fixes Errors
2. Add display to clan tag
3. Fix item highlighting
