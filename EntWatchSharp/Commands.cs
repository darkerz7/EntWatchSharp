using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using EntWatchSharp.Helpers;
using EntWatchSharp.Items;
using EntWatchSharp.Modules;
using EntWatchSharp.Modules.Eban;
using System.Globalization;
using static EntWatchSharp.Helpers.FindTarget;
using static EntWatchSharp.Modules.Eban.EbanDB;

namespace EntWatchSharp
{
    public partial class EntWatchSharp : BasePlugin
	{
		void UnRegCommands()
		{
			RemoveCommand("ew_reload", OnEWReload);
			RemoveCommand("css_ereload", OnEWReload);
			RemoveCommand("ew_showitems", OnEWShow);
			RemoveCommand("css_eshowitems", OnEWShow);
			RemoveCommand("ew_showscheme", OnEWScheme);
			RemoveCommand("css_eshowscheme", OnEWScheme);
			RemoveCommand("ehud", OnEWChangeHud);
			RemoveCommand("ehud_pos", OnEWChangeHudPos);
			RemoveCommand("ehud_color", OnEWChangeHudColor);
			RemoveCommand("ehud_refresh", OnEWChangeHudRefresh);
			RemoveCommand("ehud_sheet", OnEWChangeHudSheet);
			RemoveCommand("css_epf", OnEWChangePlayerFormat);
			RemoveCommand("eup", OnEWChangeUsePriority);
			RemoveCommand("ew_ban", OnEWBan);
			RemoveCommand("css_eban", OnEWBan);
			RemoveCommand("ew_unban", OnEWUnBan);
			RemoveCommand("css_eunban", OnEWUnBan);
			RemoveCommand("ew_status", OnEWStatus);
			RemoveCommand("css_estatus", OnEWStatus);
			RemoveCommand("ew_banlist", OnEWBanList);
			RemoveCommand("css_ebanlist", OnEWBanList);
			RemoveCommand("ew_transfer", OnEWTransfer);
			RemoveCommand("css_etransfer", OnEWTransfer);
			RemoveCommand("ew_spawn", OnEWSpawn);
			RemoveCommand("css_espawn", OnEWSpawn);
			RemoveCommand("ew_list", OnEWList);
			RemoveCommand("css_elist", OnEWList);
		}

		[ConsoleCommand("ew_reload", "Reloads config and Scheme")]
		[ConsoleCommand("css_ereload", "Reloads config and Scheme")]
		[RequiresPermissions("@css/ew_reload")]
#nullable enable
		public void OnEWReload(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (player != null && !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			EW.CleanData();
			EW.LoadScheme();
			EW.LoadConfig();
			UI.EWReplyInfo(player, "Reply.Reload_configs", bConsole);
		}

		[ConsoleCommand("ew_showitems", "Shows a list of spawned items")]
		[ConsoleCommand("css_eshowitems", "Shows a list of spawned items")]
		[RequiresPermissions("@css/ew_reload")]
#nullable enable
		public void OnEWShow(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (player != null && !player.IsValid) return;
			if (!EW.g_CfgLoaded) return;

			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			foreach (Item ItemTest in EW.g_ItemList.ToList())
			{
				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}Item: {EW.g_Scheme.color_tag}{ItemTest.Name} {EW.g_Scheme.color_warning}Index: {EW.g_Scheme.color_tag}{ItemTest.WeaponHandle.Index} {EW.g_Scheme.color_warning}Owner: {EW.g_Scheme.color_tag}{(ItemTest.Owner == null ? "Null" : ItemTest.Owner.PlayerName)}", bConsole);
				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}Buttons: ", bConsole);
				foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
				{
					if (AbilityTest.Entity != null)
					{
						string sMathID = "";
						if (!string.IsNullOrEmpty(AbilityTest.MathID) && !string.Equals(AbilityTest.MathID, "0")) sMathID = $" {EW.g_Scheme.color_warning}MathID: {EW.g_Scheme.color_tag}{AbilityTest.MathID}";
						if (AbilityTest.MathCounter != null) sMathID += $" {EW.g_Scheme.color_warning}MathCounterID: {EW.g_Scheme.color_tag}{AbilityTest.MathCounter.Index}";
						UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}BI: {EW.g_Scheme.color_tag}{AbilityTest.Entity.Index} {EW.g_Scheme.color_warning}Name: {EW.g_Scheme.color_tag}{AbilityTest.Name} {EW.g_Scheme.color_warning}ButtonID: {EW.g_Scheme.color_tag}{AbilityTest.ButtonID} {EW.g_Scheme.color_warning}ButtonClass: {EW.g_Scheme.color_tag}{AbilityTest.ButtonClass} {EW.g_Scheme.color_warning}Chat_Uses: {EW.g_Scheme.color_tag}{AbilityTest.Chat_Uses} {EW.g_Scheme.color_warning}Mode: {EW.g_Scheme.color_tag}{AbilityTest.Mode} {EW.g_Scheme.color_warning}MaxUses: {EW.g_Scheme.color_tag}{AbilityTest.MaxUses} {EW.g_Scheme.color_warning}CoolDown: {EW.g_Scheme.color_tag}{AbilityTest.CoolDown}{sMathID}", bConsole);
					}
				}
				UI.ReplyToCommand(player, " ", bConsole);
			}
		}
		[ConsoleCommand("ew_showscheme", "Shows the scheme")]
		[ConsoleCommand("css_eshowscheme", "Shows the scheme")]
		[RequiresPermissions("@css/ew_reload")]
#nullable enable
		public void OnEWScheme(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (player != null && !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}color_tag: {EW.g_Scheme.color_tag}{UI.ReplaceSpecial(EW.g_Scheme.color_tag)}", bConsole);
			UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}color_name: {EW.g_Scheme.color_name}{UI.ReplaceSpecial(EW.g_Scheme.color_name)}", bConsole);
			UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}color_steamid: {EW.g_Scheme.color_steamid}{UI.ReplaceSpecial(EW.g_Scheme.color_steamid)}", bConsole);
			UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}color_use: {EW.g_Scheme.color_use}{UI.ReplaceSpecial(EW.g_Scheme.color_use)}", bConsole);
			UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}color_pickup: {EW.g_Scheme.color_pickup}{UI.ReplaceSpecial(EW.g_Scheme.color_pickup)}", bConsole);
			UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}color_drop: {EW.g_Scheme.color_drop}{UI.ReplaceSpecial(EW.g_Scheme.color_drop)}", bConsole);
			UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}color_disconnect: {EW.g_Scheme.color_disconnect}{UI.ReplaceSpecial(EW.g_Scheme.color_disconnect)}", bConsole);
			UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}color_death: {EW.g_Scheme.color_death}{UI.ReplaceSpecial(EW.g_Scheme.color_death)}", bConsole);
			UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}color_warning: {EW.g_Scheme.color_warning}{UI.ReplaceSpecial(EW.g_Scheme.color_warning)}", bConsole);
			UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}color_enabled: {EW.g_Scheme.color_enabled}{UI.ReplaceSpecial(EW.g_Scheme.color_enabled)}", bConsole);
			UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}color_disabled: {EW.g_Scheme.color_disabled}{UI.ReplaceSpecial(EW.g_Scheme.color_disabled)}", bConsole);
			UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}server_name: {EW.g_Scheme.color_tag}{EW.g_Scheme.server_name}", bConsole);
		}

		[ConsoleCommand("ehud", "Allows the player to switch the HUD")]
		[ConsoleCommand("css_hud", "Allows the player to switch the HUD")]
		[CommandHelper(minArgs: 1, usage: "[number]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
#nullable enable
		public void OnEWChangeHud(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (EW._PlayerSettingsAPI == null || player == null || !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			if (!EW.CheckDictionary(player))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			try
			{
				if (!Int32.TryParse(command.GetArg(1), out int number)) number = 0;
				if (number >= 0 && number <= 3)
				{
					EW.g_EWPlayer[player].SwitchHud(player, number);

					EW._PlayerSettingsAPI.SetPlayerSettingsValue(player, "EW_HUD_Type", number.ToString());

					string sMessage = "";
					sMessage = number switch
					{
						0 => $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Hud.Type"]} {EW.g_Scheme.color_disabled}{Strlocalizer["All.Disabled"]}",
						1 => $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Hud.Type"]} {EW.g_Scheme.color_enabled}{Strlocalizer["All.Enabled"]} {EW.g_Scheme.color_warning}(Center)",
						2 => $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Hud.Type"]} {EW.g_Scheme.color_enabled}{Strlocalizer["All.Enabled"]} {EW.g_Scheme.color_warning}(Alert)",
						3 => $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Hud.Type"]} {EW.g_Scheme.color_enabled}{Strlocalizer["All.Enabled"]} {EW.g_Scheme.color_warning}(WorldText)",
						_ => $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Using_number"]}",
					};
					UI.ReplyToCommand(player, sMessage, bConsole);
				}
				else UI.EWReplyInfo(player, "Reply.NotValid", bConsole);
			} catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		[ConsoleCommand("ehud_pos", "Allows the player to change the position of the HUD")]
		[ConsoleCommand("css_hudpos", "Allows the player to change the position of the HUD")]
		[CommandHelper(minArgs: 3, usage: "[X Y Z] (default: -6.5 2 7; min -200.0; max 200.0)", whoCanExecute: CommandUsage.CLIENT_ONLY)]
#nullable enable
		public void OnEWChangeHudPos(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (EW._PlayerSettingsAPI == null || player == null || !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			if (!EW.CheckDictionary(player))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			try
			{
				if (!float.TryParse(command.GetArg(1).Replace(',', '.'), NumberStyles.Any, EW.cultureEN, out float fX)) fX = -6.5f;
				if (!float.TryParse(command.GetArg(2).Replace(',', '.'), NumberStyles.Any, EW.cultureEN, out float fY)) fY = 2.0f;
				if (!float.TryParse(command.GetArg(3).Replace(',', '.'), NumberStyles.Any, EW.cultureEN, out float fZ)) fZ = 7.0f;
				fX = (float)Math.Round(fX, 2);
				fY = (float)Math.Round(fY, 2);
				fZ = (float)Math.Round(fZ, 2);
				if (fX >= -200.0f && fX <= 200.0f && fY >= -200.0f && fY <= 200.0f && fZ >= -200.0f && fZ <= 200.0f)
				{
					EW.g_EWPlayer[player].HudPlayer.fXEntity = fX;
					EW.g_EWPlayer[player].HudPlayer.fYEntity = fY;
					EW.g_EWPlayer[player].HudPlayer.fZEntity = fZ;

					if (EW.g_EWPlayer[player].HudPlayer is HudWorldText) EW.g_EWPlayer[player].SwitchHud(player, 3);

					string sCookie = $"{fX.ToString(EW.cultureEN)}_{fY.ToString(EW.cultureEN)}_{fZ.ToString(EW.cultureEN)}";
					EW._PlayerSettingsAPI.SetPlayerSettingsValue(player, "EW_HUD_Pos", sCookie);

					UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Hud.Position"]} {EW.g_Scheme.color_enabled}X: {fX} Y: {fY} Z: {fZ}", bConsole);
				}
				else UI.EWReplyInfo(player, "Reply.NotValid", bConsole);
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		[ConsoleCommand("ehud_color", "Allows the player to change the color of the HUD")]
		[ConsoleCommand("css_hudcolor", "Allows the player to change the color of the HUD")]
		[CommandHelper(minArgs: 4, usage: "[R G B A] (default: 255 255 255 255; min 0; max 255)", whoCanExecute: CommandUsage.CLIENT_ONLY)]
#nullable enable
		public void OnEWChangeHudColor(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (EW._PlayerSettingsAPI == null || player == null || !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			if (!EW.CheckDictionary(player))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			try
			{
				if (!int.TryParse(command.GetArg(1), out int iRed)) iRed = 255;
				if (!int.TryParse(command.GetArg(2), out int iGreen)) iGreen = 255;
				if (!int.TryParse(command.GetArg(3), out int iBlue)) iBlue = 255;
				if (!int.TryParse(command.GetArg(4), out int iAlpha)) iAlpha = 255;
				if (iRed >= 0 && iRed <= 255 && iGreen >= 0 && iGreen <= 255 && iBlue >= 0 && iBlue <= 255 && iAlpha >= 0 && iAlpha <= 255)
				{
					EW.g_EWPlayer[player].HudPlayer.colorEntity = [iRed, iGreen, iBlue, iAlpha];
					if (EW.g_EWPlayer[player].HudPlayer is HudWorldText) EW.g_EWPlayer[player].SwitchHud(player, 3);

					string sCookie = $"{iRed}_{iGreen}_{iBlue}_{iAlpha}";
					EW._PlayerSettingsAPI.SetPlayerSettingsValue(player, "EW_HUD_Color", sCookie);

					UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Hud.Color"]} {EW.g_Scheme.color_enabled}R: {iRed} G: {iGreen} B: {iBlue} A: {iAlpha}", bConsole);
				}
				else UI.EWReplyInfo(player, "Reply.NotValid", bConsole);
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		[ConsoleCommand("ehud_size", "Allows the player to change the size of the HUD")]
		[ConsoleCommand("css_hudsize", "Allows the player to change the size of the HUD")]
		[CommandHelper(minArgs: 1, usage: "[size] (default: 54; min 16; max 128)", whoCanExecute: CommandUsage.CLIENT_ONLY)]
#nullable enable
		public void OnEWChangeHudSize(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (EW._PlayerSettingsAPI == null || player == null || !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			if (!EW.CheckDictionary(player))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			try
			{
				if (!Int32.TryParse(command.GetArg(1), out int number)) number = 0;
				if (number >= 16 && number <= 128)
				{
					EW.g_EWPlayer[player].HudPlayer.iSize = number;
					if (EW.g_EWPlayer[player].HudPlayer is HudWorldText) EW.g_EWPlayer[player].SwitchHud(player, 3);
					EW._PlayerSettingsAPI.SetPlayerSettingsValue(player, "EW_HUD_Size", number.ToString());
					UI.EWReplyInfo(player, "Reply.Hud.Size", bConsole, EW.g_Scheme.color_enabled, number);
				}
				else UI.EWReplyInfo(player, "Reply.NotValid", bConsole);
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		[ConsoleCommand("ehud_refresh", "Allows the player to change the time it takes to scroll through the list")]
		[ConsoleCommand("css_hudrefresh", "Allows the player to change the time it takes to scroll through the list")]
		[CommandHelper(minArgs: 1, usage: "[sec] (default: 3; min 1; max 10)", whoCanExecute: CommandUsage.CLIENT_ONLY)]
#nullable enable
		public void OnEWChangeHudRefresh(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (EW._PlayerSettingsAPI == null || player == null || !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			if (!EW.CheckDictionary(player))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			try
			{
				if (!Int32.TryParse(command.GetArg(1), out int number)) number = 0;
				if (number >= 1 && number <= 10)
				{
					EW.g_EWPlayer[player].HudPlayer.iRefresh = number;
					EW._PlayerSettingsAPI.SetPlayerSettingsValue(player, "EW_HUD_Refresh", number.ToString());
					UI.EWReplyInfo(player, "Reply.Hud.Refresh", bConsole, EW.g_Scheme.color_enabled, number, EW.g_Scheme.color_warning);
				} else UI.EWReplyInfo(player, "Reply.NotValid", bConsole);
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		[ConsoleCommand("ehud_sheet", "Allows the player to change the number of items on the sheet")]
		[ConsoleCommand("css_hudsheet", "Allows the player to change the number of items on the sheet")]
		[CommandHelper(minArgs: 1, usage: "[count] (default: 5; min 1; max 15)", whoCanExecute: CommandUsage.CLIENT_ONLY)]
#nullable enable
		public void OnEWChangeHudSheet(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (EW._PlayerSettingsAPI == null || player == null || !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			if (!EW.CheckDictionary(player))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			try
			{
				if (!Int32.TryParse(command.GetArg(1), out int number)) number = 0;
				if (number >= 1 && number <= 15)
				{
					EW.g_EWPlayer[player].HudPlayer.iSheetMax = number;
					EW._PlayerSettingsAPI.SetPlayerSettingsValue(player, "EW_HUD_Sheet", number.ToString());
					UI.EWReplyInfo(player, "Reply.Hud.Sheet", bConsole, EW.g_Scheme.color_enabled, number, EW.g_Scheme.color_warning);
				}
				else UI.EWReplyInfo(player, "Reply.NotValid", bConsole);
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		[ConsoleCommand("epf", "Allows the player to change the player display format")]
		[ConsoleCommand("css_epf", "Allows the player to change the player display format")]
		[CommandHelper(minArgs: 1, usage: "[number] (default: 3; min 0; max 3)", whoCanExecute: CommandUsage.CLIENT_ONLY)]
#nullable enable
		public void OnEWChangePlayerFormat(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (EW._PlayerSettingsAPI == null || player == null || !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			if (!EW.CheckDictionary(player))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			try
			{
				if (!Int32.TryParse(command.GetArg(1), out int number)) number = 3;
				if (number >= 0 && number <= 3)
				{
					EW.g_EWPlayer[player].PFormatPlayer = number;
					EW._PlayerSettingsAPI.SetPlayerSettingsValue(player, "EW_PInfo_Format", number.ToString());
					switch (number)
					{
						case 1: UI.EWReplyInfo(player, "Reply.PlayerInfo.UserID", bConsole, EW.g_Scheme.color_enabled); break;
						case 2: UI.EWReplyInfo(player, "Reply.PlayerInfo.SteamID", bConsole, EW.g_Scheme.color_enabled); break;
						case 3: UI.EWReplyInfo(player, "Reply.PlayerInfo.Full", bConsole, EW.g_Scheme.color_enabled); break;
						default: UI.EWReplyInfo(player, "Reply.PlayerInfo.NicknameOnly", bConsole, EW.g_Scheme.color_enabled); break;
					}
				}
				else UI.EWReplyInfo(player, "Reply.NotValid", bConsole);
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		[ConsoleCommand("eup", "Allows the player to use UsePriority")]
		[ConsoleCommand("css_eup", "Allows the player to use UsePriority")]
		[CommandHelper(minArgs: 0, usage: "[bool]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
#nullable enable
		public void OnEWChangeUsePriority(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (EW._PlayerSettingsAPI == null || player == null || !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			if (!EW.CheckDictionary(player))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			try
			{
				bool bNewValue = EW.g_EWPlayer[player].UsePriorityPlayer.Activate;

				string sValue = command.GetArg(1);
				if (!string.IsNullOrEmpty(sValue))
				{
					bNewValue = sValue.Contains("true", StringComparison.OrdinalIgnoreCase) || string.Equals(sValue, "1");
				}
				else bNewValue = !bNewValue;

				EW.g_EWPlayer[player].UsePriorityPlayer.Activate = bNewValue;

				if (bNewValue)
				{
					EW._PlayerSettingsAPI.SetPlayerSettingsValue(player, "EW_Use_Priority", "1");
					UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Use_Priority"]} {EW.g_Scheme.color_enabled}{Strlocalizer["All.Enabled"]}", bConsole);
				}
				else
				{
					EW._PlayerSettingsAPI.SetPlayerSettingsValue(player, "EW_Use_Priority", "0");
					UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Use_Priority"]} {EW.g_Scheme.color_disabled}{Strlocalizer["All.Disabled"]}", bConsole);
				}
			}catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		//Eban Start
		[ConsoleCommand("ew_ban", "Allows the admin to restrict items for the player")]
		[ConsoleCommand("css_eban", "Allows the admin to restrict items for the player")]
		[RequiresPermissions("@css/ew_ban")]
		[CommandHelper(minArgs: 1, usage: "<#userid|name|#steamid> [<time>] [<reason>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWBan(CCSPlayerController? admin, CommandInfo command)
#nullable disable
		{
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			(List<CCSPlayerController> players, string _) = Find(admin, command, 1, true, true, MultipleFlags.NORMAL, false);

			OfflineBan target = null;

			if (players.Count > 0)
			{
				CCSPlayerController targetOnline = players.Single();

				if (!AdminManager.CanPlayerTarget(admin, targetOnline))
				{
					UI.EWReplyInfo(admin, "Reply.You_cannot_target", bConsole);
					return;
				}

				if (!EW.CheckDictionary(targetOnline))
				{
					UI.EWReplyInfo(admin, "Info.Error", bConsole, "Player not found in dictionary");
					return;
				}

				if (EW.g_EWPlayer[targetOnline].BannedPlayer.bBanned)
				{
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(admin, UI.PlayerInfoFormat(targetOnline))} {Strlocalizer["Reply.Eban.Has_a_Restrict"]}", bConsole);
					return;
				}

				foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
				{
					if (OfflineTest.UserID == targetOnline.UserId)
					{
						target = OfflineTest;
						break;
					}
				}
			} else
			{
				target = OfflineFunc.FindTarget(admin, command.GetArg(1), bConsole);
			}

			if (target == null)
			{
				UI.EWReplyInfo(admin, "Reply.No_matching_client", bConsole);
				return;
			}

			int time = Cvar.BanTime;
			if (command.ArgCount >= 2)
			{
				if (!int.TryParse(command.GetArg(2), out int timeparse))
				{
					UI.EWReplyInfo(admin, "Reply.Must_be_an_integer", bConsole);
					return;
				}
				time = timeparse;
			}

			if (time == 0 && !AdminManager.PlayerHasPermissions(admin, "@css/ew_ban_perm"))
			{
				UI.EWReplyInfo(admin, "Reply.Eban.Access.Permanent", bConsole);
				return;
			}

			if (time > Cvar.BanLong && !AdminManager.PlayerHasPermissions(admin, "@css/ew_ban_long"))
			{
				UI.EWReplyInfo(admin, "Reply.Eban.Access.Long", bConsole, Cvar.BanLong);
				return;
			}

			string reason = command.GetArg(3);
			if (string.IsNullOrEmpty(reason)) reason = Cvar.BanReason;

			EbanPlayer ebanPlayer = target.Online ? EW.g_EWPlayer[target.Player].BannedPlayer : new EbanPlayer();
			if (ebanPlayer.SetBan(admin != null ? admin.PlayerName : "Console", admin != null ? EW.ConvertSteamID64ToSteamID(admin.SteamID.ToString()) : "SERVER", target.Name, target.SteamID, time, reason))
				UI.EWSysInfo("Reply.Eban.Ban.Success", 6);
			else
			{
				UI.EWSysInfo("Reply.Eban.Ban.Failed", 15);
				return;
			}

			UI.EWChatAdminBan(UI.PlayerInfoFormat(admin), target.Online ? UI.PlayerInfoFormat(target.Player) : UI.PlayerInfoFormat(target.Name, target.SteamID), reason, true);
		}

		[ConsoleCommand("ew_unban", "Allows the admin to remove the item restriction for a player")]
		[ConsoleCommand("css_eunban", "Allows the admin to remove the item restriction for a player")]
		[RequiresPermissions("@css/ew_unban")]
		[CommandHelper(minArgs: 1, usage: "<#userid|name|#steamid> [<reason>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWUnBan(CCSPlayerController? admin, CommandInfo command)
#nullable disable
		{
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			(List<CCSPlayerController> players, string _) = Find(admin, command, 1, true, true, MultipleFlags.NORMAL, false);

			EbanPlayer target = new();
			string sTarget = command.GetArg(1);

			bool bOnline = players.Count > 0;

			if (bOnline)
			{
				CCSPlayerController targetController = players.Single();
				if (!AdminManager.CanPlayerTarget(admin, targetController))
				{
					UI.EWReplyInfo(admin, "Reply.You_cannot_target", bConsole);
					return;
				}
				if (!EW.CheckDictionary(targetController))
				{
					UI.EWReplyInfo(admin, "Info.Error", bConsole, "Player not found in dictionary");
					return;
				}
				target.bBanned = EW.g_EWPlayer[targetController].BannedPlayer.bBanned;
				target.sAdminName = EW.g_EWPlayer[targetController].BannedPlayer.sAdminSteamID;
				target.sAdminSteamID = EW.g_EWPlayer[targetController].BannedPlayer.sAdminSteamID;
				target.iDuration = EW.g_EWPlayer[targetController].BannedPlayer.iDuration;
				target.iTimeStamp_Issued = EW.g_EWPlayer[targetController].BannedPlayer.iTimeStamp_Issued;
				target.sReason = EW.g_EWPlayer[targetController].BannedPlayer.sReason;
				target.sClientName = targetController.PlayerName;
				target.sClientSteamID = EW.ConvertSteamID64ToSteamID(targetController.SteamID.ToString());
			}

			string reason = command.GetArg(2);
			if (string.IsNullOrEmpty(reason)) reason = Cvar.UnBanReason;
			if (bOnline) UnBanComm(admin, players.Single(), target, reason, bConsole);
			else if (sTarget.StartsWith("#steam_", StringComparison.OrdinalIgnoreCase))
			{
				EbanPlayer.GetBan(sTarget[1..], admin, reason, bConsole, GetBanComm_Handler);
			} else UI.EWReplyInfo(admin, "Reply.No_matching_client", bConsole);
		}
#nullable enable
		GetBanCommFunc GetBanComm_Handler = (string sClientSteamID, CCSPlayerController? admin, string reason, bool bConsole, List<List<string>> DBQuery_Result) =>
#nullable disable
		{
			if (DBQuery_Result.Count > 0)
			{
				EbanPlayer target = new()
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
				UnBanComm(admin, null, target, reason, bConsole);
				return;
			}
			UnBanComm(admin, null, null, reason, bConsole);
		};
#nullable enable
		static void UnBanComm(CCSPlayerController? admin, CCSPlayerController? player, EbanPlayer? target, string reason, bool bConsole)
#nullable disable
		{
			if (target == null)
			{
				UI.EWReplyInfo(admin, "Reply.No_matching_client", bConsole);
				return;
			}

			if (!target.bBanned)
			{
				UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(admin, UI.PlayerInfoFormat(target.sClientName, target.sClientSteamID))} {Strlocalizer["Reply.Eban.Can_pickup"]}", bConsole);
				return;
			}

			if (target.iDuration == 0 && !AdminManager.PlayerHasPermissions(admin, "@css/ew_unban_perm"))
			{
				UI.EWReplyInfo(admin, "Reply.Eban.Access.UnPermanent", bConsole);
				return;
			}

			if (admin != null && !string.Equals(target.sAdminSteamID, EW.ConvertSteamID64ToSteamID(admin.SteamID.ToString())) && !AdminManager.PlayerHasPermissions(admin, "@css/ew_unban_other"))
			{
				UI.EWReplyInfo(admin, "Reply.Eban.Access.Other", bConsole);
				return;
			}

			if (target.UnBan(admin != null ? admin.PlayerName : "Console", admin != null ? EW.ConvertSteamID64ToSteamID(admin.SteamID.ToString()) : "SERVER", target.sClientSteamID, reason))
			{
				if (player != null) EW.g_EWPlayer[player].BannedPlayer.bBanned = false;
				UI.EWSysInfo("Reply.Eban.UnBan.Success", 6);
			}
			else
			{
				UI.EWSysInfo("Reply.Eban.UnBan.Failed", 15);
				return;
			}

			UI.EWChatAdminBan(UI.PlayerInfoFormat(admin), UI.PlayerInfoFormat(target.sClientName, target.sClientSteamID), reason, false);
		}

		[ConsoleCommand("ew_status", "Allows the player to view the restrictions")]
		[ConsoleCommand("css_estatus", "Allows the player to view the restrictions")]
		[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
#nullable enable
		public void OnEWStatus(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (player != null && !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			CCSPlayerController target = player;
			if (command.ArgCount > 1)
			{
				(List<CCSPlayerController> players, string _) = Find(player, command, 1, true, false, MultipleFlags.NORMAL);

				if (players.Count == 0) return;

				target = players.Single();
			}
			if (target == null || !EW.CheckDictionary(target))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			if (EW.g_EWPlayer[target].BannedPlayer.bBanned)
			{
				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(player, UI.PlayerInfoFormat(target))} {Strlocalizer["Reply.Eban.Has_a_Restrict"]}", bConsole);

				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Admin"]}: {EW.g_Scheme.color_name}{UI.PlayerInfo(player, UI.PlayerInfoFormat(EW.g_EWPlayer[target].BannedPlayer.sAdminName, EW.g_EWPlayer[target].BannedPlayer.sAdminSteamID))}", bConsole);

				switch(EW.g_EWPlayer[target].BannedPlayer.iDuration)
				{
					case -1: UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_enabled}{Strlocalizer["Reply.Eban.Temporary"]}", bConsole); break;
					case 0: UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_disabled}{Strlocalizer["Reply.Eban.Permanently"]}", bConsole); break;
					default: UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_disabled}{EW.g_EWPlayer[target].BannedPlayer.iDuration} {Strlocalizer["Reply.Eban.Minutes"]}", bConsole); break;
				}

				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Eban.Expires"]}: {EW.g_Scheme.color_disabled}{DateTimeOffset.FromUnixTimeSeconds(EW.g_EWPlayer[target].BannedPlayer.iTimeStamp_Issued)}", bConsole);

				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Eban.Reason"]}: {EW.g_Scheme.color_disabled}{EW.g_EWPlayer[target].BannedPlayer.sReason}", bConsole);
			} else
			{
				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(player, UI.PlayerInfoFormat(target))} {Strlocalizer["Reply.Eban.Can_pickup"]}", bConsole);
			}
		}
		[ConsoleCommand("ew_banlist", "Displays a list of restrictions")]
		[ConsoleCommand("css_ebanlist", "Displays a list of restrictions")]
		[RequiresPermissions("@css/ew_ban")]
		[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWBanList(CCSPlayerController? admin, CommandInfo command)
#nullable disable
		{
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			UI.EWReplyInfo(admin, "Reply.Eban.List", bConsole);
			int iCount = 0;
			foreach(var pair in EW.g_EWPlayer)
			{
				if (pair.Value.BannedPlayer.bBanned)
				{
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}***{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(admin, UI.PlayerInfoFormat(pair.Key))}{EW.g_Scheme.color_warning}***", bConsole);
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Admin"]} {EW.g_Scheme.color_name}{UI.PlayerInfo(admin, UI.PlayerInfoFormat(pair.Value.BannedPlayer.sAdminName, pair.Value.BannedPlayer.sAdminSteamID))}", bConsole);
					switch (pair.Value.BannedPlayer.iDuration)
					{
						case -1: UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_enabled}{Strlocalizer["Reply.Eban.Temporary"]}", bConsole); break;
						case 0: UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_disabled}{Strlocalizer["Reply.Eban.Permanently"]}", bConsole); break;
						default: UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_disabled}{pair.Value.BannedPlayer.iDuration} {Strlocalizer["Reply.Eban.Minutes"]}", bConsole); break;
					}
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Expires"]}: {EW.g_Scheme.color_disabled}{DateTimeOffset.FromUnixTimeSeconds(pair.Value.BannedPlayer.iTimeStamp_Issued)}", bConsole);

					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Reason"]}: {EW.g_Scheme.color_disabled}{pair.Value.BannedPlayer.sReason}", bConsole);

					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|__________________________________________", bConsole);
					iCount++;
				}
			}
			if (iCount == 0) UI.EWReplyInfo(admin, "Reply.Eban.NoPlayers", bConsole);

			/*UI.EWReplyInfo(admin, "Reply.Eban.List", bConsole);
			int iCount = 0;
			Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(target =>
			{
				if (EW.CheckDictionary(target) && EW.g_EWPlayer[target].BannedPlayer.bBanned)
				{
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}***{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(admin, UI.PlayerInfoFormat(target))}{EW.g_Scheme.color_warning}***", bConsole);
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Admin"]} {EW.g_Scheme.color_name}{UI.PlayerInfo(admin, UI.PlayerInfoFormat(EW.g_EWPlayer[target].BannedPlayer.sAdminName, EW.g_EWPlayer[target].BannedPlayer.sAdminSteamID))}", bConsole);
					switch (EW.g_EWPlayer[target].BannedPlayer.iDuration)
					{
						case -1: UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_enabled}{Strlocalizer["Reply.Eban.Temporary"]}", bConsole); break;
						case 0: UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_disabled}{Strlocalizer["Reply.Eban.Permanently"]}", bConsole); break;
						default: UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_disabled}{EW.g_EWPlayer[target].BannedPlayer.iDuration} {Strlocalizer["Reply.Eban.Minutes"]}", bConsole); break;
					}
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Expires"]}: {EW.g_Scheme.color_disabled}{DateTimeOffset.FromUnixTimeSeconds(EW.g_EWPlayer[target].BannedPlayer.iTimeStamp_Issued)}", bConsole);

					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Reason"]}: {EW.g_Scheme.color_disabled}{EW.g_EWPlayer[target].BannedPlayer.sReason}", bConsole);
					
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|__________________________________________", bConsole);
					iCount++;
				}
			});
			if (iCount == 0) UI.EWReplyInfo(admin, "Reply.Eban.NoPlayers", bConsole);*/
		}
		//Eban End

		[ConsoleCommand("ew_transfer", "Allows the admin to transfer items")]
		[ConsoleCommand("css_etransfer", "Allows the admin to transfer items")]
		[RequiresPermissions("@css/ew_transfer")]
		[CommandHelper(minArgs: 2, usage: "<owner>/$<itemname> <receiver>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWTransfer(CCSPlayerController? admin, CommandInfo command)
#nullable disable
		{
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			string sItemName = command.GetArg(1);
			CCSPlayerController target = null;
			Item item = null;
			if (sItemName[0] == '$')
			{
				sItemName = sItemName.Remove(0, 1).ToLower();
				foreach (Item ItemTest in EW.g_ItemList.ToList())
				{
					if (ItemTest.Name.Contains(sItemName, StringComparison.OrdinalIgnoreCase) || ItemTest.ShortName.Contains(sItemName, StringComparison.OrdinalIgnoreCase))
					{
						target = ItemTest.Owner;
						item = ItemTest;
						break;
					}
				}
				if(item == null)
				{
					UI.EWReplyInfo(admin, "Reply.Transfer.InvalidItemName", bConsole);
					return;
				}
			} else
			{
				(List<CCSPlayerController> players, string _) = Find(admin, command, 1, true, true, MultipleFlags.IGNORE_DEAD_PLAYERS);
				if (players.Count == 0) return;
				target = players.Single();
			}

			if (target != null && !AdminManager.CanPlayerTarget(admin, target))
			{
				UI.EWReplyInfo(admin, "Reply.You_cannot_target", bConsole);
				return;
			}

			(List<CCSPlayerController> players1, string _) = Find(admin, command, 2, true, false, MultipleFlags.IGNORE_DEAD_PLAYERS);
			if (players1.Count == 0) return;
			CCSPlayerController receiver = players1.Single();

			if (!EW.CheckDictionary(receiver))
			{
				UI.EWReplyInfo(admin, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}

			if (target != null)
			{
				if (target == receiver)
				{
					UI.EWReplyInfo(admin, "Reply.Transfer.AlreadyOwns", bConsole);
					return;
				}

				if (target.TeamNum != receiver.TeamNum)
				{
					UI.EWReplyInfo(admin, "Reply.Transfer.Differsteam", bConsole);
					return;
				}
			}

			if (EW.g_EWPlayer[receiver].BannedPlayer.bBanned)
			{
				UI.EWReplyInfo(admin, "Reply.Transfer.ReceiverHasBan", bConsole);
				return;
			}

			if (item == null)
			{
				Transfer.Target(admin, target, receiver, bConsole);
			}
			else
			{
				Transfer.ItemName(admin, item, receiver, bConsole);
			}
		}

		[ConsoleCommand("ew_spawn", "Allows the admin to spawn items")]
		[ConsoleCommand("css_espawn", "Allows the admin to spawn items")]
		[RequiresPermissions("@css/ew_spawn")]
		[CommandHelper(minArgs: 2, usage: "<itemname> <receiver> [<strip>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWSpawn(CCSPlayerController? admin, CommandInfo command)
#nullable disable
		{
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			string sItemName = command.GetArg(1);

			if(string.IsNullOrEmpty(sItemName))
			{
				UI.EWReplyInfo(admin, "Reply.Spawn.BadItemName", bConsole);
				return;
			}

			(List<CCSPlayerController> players, string _) = Find(admin, command, 2, true, false, MultipleFlags.IGNORE_DEAD_PLAYERS);
			if (players.Count == 0) return;
			CCSPlayerController receiver = players.Single();

			if (!EW.CheckDictionary(receiver))
			{
				UI.EWReplyInfo(admin, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}

			if (EW.g_EWPlayer[receiver].BannedPlayer.bBanned)
			{
				UI.EWReplyInfo(admin, "Reply.Spawn.ReceiverHasBan", bConsole);
				return;
			}

			bool bStrip = false;
			string sStrip = command.GetArg(3);
			if (!string.IsNullOrEmpty(sStrip)) bStrip = sStrip.Contains("true", StringComparison.OrdinalIgnoreCase) || string.Equals(sStrip, "1");

			SpawnItem.Spawn(admin, receiver, sItemName, bStrip, bConsole);
		}

		[ConsoleCommand("ew_list", "Shows a list of players including those who have disconnected")]
		[ConsoleCommand("css_elist", "Shows a list of players including those who have disconnected")]
		[RequiresPermissions("@css/ew_ban")]
		[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWList(CCSPlayerController? admin, CommandInfo command)
#nullable disable
		{
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			
			UI.EWReplyInfo(admin, "Reply.Offline.Info", bConsole);

			int iCount = 0;
			double CurrentTime = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
			{
				iCount++;
				if (OfflineTest.Online)
				{
					UI.EWReplyInfo(admin, "Reply.Offline.OnServer", bConsole, iCount, OfflineTest.Name, OfflineTest.UserID, OfflineTest.SteamID, !string.IsNullOrEmpty(OfflineTest.LastItem) ? OfflineTest.LastItem : "-");
				} else
				{
					UI.EWReplyInfo(admin, "Reply.Offline.Leave", bConsole, iCount, OfflineTest.Name, OfflineTest.UserID, OfflineTest.SteamID, !string.IsNullOrEmpty(OfflineTest.LastItem) ? OfflineTest.LastItem : "-", (int)((CurrentTime - OfflineTest.TimeStamp_Start) / 60));
				}
			}
		}

		/*[ConsoleCommand("ew_testhud", "This is an example command description")]
		[RequiresPermissions("@css/ew_reload")]
		public void OnEWTestHud(CCSPlayerController? player, CommandInfo command)
		{
			//blue
			//player.PrintToCenter("Test PrintToCenter");
			//red
			//player.PrintToCenterAlert("Test PrintToCenterAlert");
			//disappears quickly
			//player.PrintToCenterHtml("Test PrintToCenterHtml", 200000);
			Utilities.GetPlayers().ForEach(pl =>
			{
				pl.PrintToCenter($"Player: {pl.PlayerName} Hud: {EW.g_HudPlayer[pl].GetType()}");
			});
		}*/
	}
}
