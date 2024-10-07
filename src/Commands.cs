using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using EntWatchSharp.Items;
using static EntWatchSharp.Helpers.FindTarget;
using EntWatchSharp.Helpers;
using EntWatchSharp.Modules;
using EntWatchSharp.Modules.Eban;

namespace EntWatchSharp
{
    public partial class EntWatchSharp : BasePlugin
	{
		void UnRegCommands()
		{
			RemoveCommand("ew_reload", OnEWReload);
			RemoveCommand("ew_showitems", OnEWShow);
			RemoveCommand("ew_showscheme", OnEWScheme);
			RemoveCommand("ehud", OnEWChangeHud);
			RemoveCommand("ehud_pos", OnEWChangeHudPos);
			RemoveCommand("ehud_refresh", OnEWChangeHudRefresh);
			RemoveCommand("ehud_sheet", OnEWChangeHudSheet);
			RemoveCommand("eup", OnEWChangeUsePriority);
			RemoveCommand("ew_ban", OnEWBan);
			RemoveCommand("ew_unban", OnEWUnBan);
			RemoveCommand("ew_status", OnEWStatus);
			RemoveCommand("ew_banlist", OnEWBanList);
			RemoveCommand("ew_transfer", OnEWTransfer);
			RemoveCommand("ew_spawn", OnEWSpawn);
			RemoveCommand("ew_list", OnEWList);
		}

		[ConsoleCommand("ew_reload", "Reloads config and Scheme")]
		[RequiresPermissions("@css/ew_reload")]
		public void OnEWReload(CCSPlayerController? player, CommandInfo command)
		{
			if (player != null && !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			EW.CleanData();
			EW.LoadScheme();
			EW.LoadConfig();
			UI.EWReplyInfo(player, "Reply.Reload_configs", bConsole);
		}

		[ConsoleCommand("ew_showitems", "Shows a list of spawned items")]
		[RequiresPermissions("@css/ew_reload")]
		public void OnEWShow(CCSPlayerController? player, CommandInfo command)
		{
			if (player != null && !player.IsValid) return;
			if (!EW.g_CfgLoaded) return;

			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			foreach (Item ItemTest in EW.g_ItemList.ToList())
			{
				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}Item: {EW.g_Scheme.color_tag}{ItemTest.Name} {EW.g_Scheme.color_warning}Index: {EW.g_Scheme.color_tag}{ItemTest.WeaponHandle.Index.ToString()} {EW.g_Scheme.color_warning}Owner: {EW.g_Scheme.color_tag}{(ItemTest.Owner == null ? "Null" : ItemTest.Owner.PlayerName)}", bConsole);
				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}Buttons: ", bConsole);
				foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
				{
					if (AbilityTest.Entity != null)
					{
						string sMathID = "";
						if (AbilityTest.MathID > 0) sMathID = $" {EW.g_Scheme.color_warning}MathID: {EW.g_Scheme.color_tag}{AbilityTest.MathID}";
						if (AbilityTest.MathCounter != null) sMathID += $" {EW.g_Scheme.color_warning}MathCounterID: {EW.g_Scheme.color_tag}{AbilityTest.MathCounter.Index}";
						UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}BI: {EW.g_Scheme.color_tag}{AbilityTest.Entity.Index} {EW.g_Scheme.color_warning}Name: {EW.g_Scheme.color_tag}{AbilityTest.Name} {EW.g_Scheme.color_warning}ButtonID: {EW.g_Scheme.color_tag}{AbilityTest.ButtonID} {EW.g_Scheme.color_warning}ButtonClass: {EW.g_Scheme.color_tag}{AbilityTest.ButtonClass} {EW.g_Scheme.color_warning}Chat_Uses: {EW.g_Scheme.color_tag}{AbilityTest.Chat_Uses} {EW.g_Scheme.color_warning}Mode: {EW.g_Scheme.color_tag}{AbilityTest.Mode} {EW.g_Scheme.color_warning}MaxUses: {EW.g_Scheme.color_tag}{AbilityTest.MaxUses} {EW.g_Scheme.color_warning}CoolDown: {EW.g_Scheme.color_tag}{AbilityTest.CoolDown}{sMathID}", bConsole);
					}
				}
				UI.ReplyToCommand(player, " ", bConsole);
			}
		}
		[ConsoleCommand("ew_showscheme", "Shows the scheme")]
		[RequiresPermissions("@css/ew_reload")]
		public void OnEWScheme(CCSPlayerController? player, CommandInfo command)
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
		[CommandHelper(minArgs: 1, usage: "[number]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
		public async void OnEWChangeHud(CCSPlayerController? player, CommandInfo command)
		{
			if (!EW.g_bAPI || player == null || !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			if (!EW.CheckDictionary(player,EW.g_HudPlayer))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			try
			{
				int number;
				if (!Int32.TryParse(command.GetArg(1), out number)) number = 0;
				if (number >= 0 && number <= 3)
				{
					EW.SwitchHud(player, number);

					await EW._CP_api.SetClientCookie(player.SteamID.ToString(), "EW_HUD_Type", number.ToString());

					string sMessage = "";
					switch (number)
					{
						case 0: sMessage = $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Hud.Type"]} {EW.g_Scheme.color_disabled}{Strlocalizer["All.Disabled"]}"; break;
						case 1: sMessage = $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Hud.Type"]} {EW.g_Scheme.color_enabled}{Strlocalizer["All.Enabled"]} {EW.g_Scheme.color_warning}(Center)"; break;
						case 2: sMessage = $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Hud.Type"]} {EW.g_Scheme.color_enabled}{Strlocalizer["All.Enabled"]} {EW.g_Scheme.color_warning}(Alert)"; break;
						case 3: sMessage = $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Hud.Type"]} {EW.g_Scheme.color_enabled}{Strlocalizer["All.Enabled"]} {EW.g_Scheme.color_warning}(WorldText)"; break;
						default: sMessage = $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Using_number"]}"; break;
					}
					UI.ReplyToCommand(player, sMessage, bConsole);
				}
				else UI.EWReplyInfo(player, "Reply.NotValid", bConsole);
			} catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		[ConsoleCommand("ehud_pos", "Allows the player to change the position of the HUD")]
		[CommandHelper(minArgs: 3, usage: "[X Y Z] (default: 50 50 50; min -200.0; max 200.0)", whoCanExecute: CommandUsage.CLIENT_ONLY)]
		public async void OnEWChangeHudPos(CCSPlayerController? player, CommandInfo command)
		{
			if (!EW.g_bAPI || player == null || !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			if (!EW.CheckDictionary(player, EW.g_HudPlayer))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			try
			{
				float fX, fY, fZ;
				if (!float.TryParse(command.GetArg(1), out fX)) fX = 50;
				if (!float.TryParse(command.GetArg(2), out fY)) fY = 50;
				if (!float.TryParse(command.GetArg(3), out fZ)) fZ = 50;
				fX = (float)Math.Round(fX, 1);
				fY = (float)Math.Round(fY, 1);
				fZ = (float)Math.Round(fZ, 1);
				if (fX >= -200.0 && fX <= 200.0 && fY >= -200.0 && fY <= 200.0 && fZ >= -200.0 && fZ <= 200.0)
				{
					EW.g_HudPlayer[player].vecEntity = new CounterStrikeSharp.API.Modules.Utils.Vector(fX, fY, fZ);
					if (EW.g_HudPlayer[player] is HudWorldText) EW.SwitchHud(player, 3);

					string sCookie = $"{fX}_{fY}_{fZ}";
					await EW._CP_api.SetClientCookie(player.SteamID.ToString(), "EW_HUD_Pos", sCookie);

					UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Hud.Position"]} {EW.g_Scheme.color_enabled}X: {fX} Y: {fY} Z: {fZ}", bConsole);
				}
				else UI.EWReplyInfo(player, "Reply.NotValid", bConsole);
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		[ConsoleCommand("ehud_refresh", "Allows the player to change the time it takes to scroll through the list")]
		[CommandHelper(minArgs: 1, usage: "[sec] (default: 3; min 1; max 10)", whoCanExecute: CommandUsage.CLIENT_ONLY)]
		public async void OnEWChangeHudRefresh(CCSPlayerController? player, CommandInfo command)
		{
			if (!EW.g_bAPI || player == null || !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			if (!EW.CheckDictionary(player, EW.g_HudPlayer))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			try
			{
				int number;
				if (!Int32.TryParse(command.GetArg(1), out number)) number = 0;
				if (number >= 1 && number <= 10)
				{
					EW.g_HudPlayer[player].iRefresh = number;
					await EW._CP_api.SetClientCookie(player.SteamID.ToString(), "EW_HUD_Refresh", number.ToString());
					UI.EWReplyInfo(player, "Reply.Hud.Refresh", bConsole, EW.g_Scheme.color_enabled, number, EW.g_Scheme.color_warning);
				} else UI.EWReplyInfo(player, "Reply.NotValid", bConsole);
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		[ConsoleCommand("ehud_sheet", "Allows the player to change the number of items on the sheet")]
		[CommandHelper(minArgs: 1, usage: "[count] (default: 5; min 1; max 15)", whoCanExecute: CommandUsage.CLIENT_ONLY)]
		public async void OnEWChangeHudSheet(CCSPlayerController? player, CommandInfo command)
		{
			if (!EW.g_bAPI || player == null || !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			if (!EW.CheckDictionary(player, EW.g_HudPlayer))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			try
			{
				int number;
				if (!Int32.TryParse(command.GetArg(1), out number)) number = 0;
				if (number >= 1 && number <= 15)
				{
					EW.g_HudPlayer[player].iSheetMax = number;
					await EW._CP_api.SetClientCookie(player.SteamID.ToString(), "EW_HUD_Sheet", number.ToString());
					UI.EWReplyInfo(player, "Reply.Hud.Sheet", bConsole, EW.g_Scheme.color_enabled, number, EW.g_Scheme.color_warning);
				}
				else UI.EWReplyInfo(player, "Reply.NotValid", bConsole);
			}
			catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		[ConsoleCommand("eup", "Allows the player to use UsePriority")]
		[CommandHelper(minArgs: 0, usage: "[bool]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
		public async void OnEWChangeUsePriority(CCSPlayerController? player, CommandInfo command)
		{
			if (!EW.g_bAPI || player == null || !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;
			if (!EW.CheckDictionary(player, EW.g_UsePriorityPlayer))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			try
			{
				bool bNewValue = EW.g_UsePriorityPlayer[player].Activate;

				string sValue = command.GetArg(1);
				if (!string.IsNullOrEmpty(sValue))
				{
					bNewValue = sValue.ToLower().Contains("true") || sValue.CompareTo("1") == 0;
				}
				else bNewValue = !bNewValue;

				EW.g_UsePriorityPlayer[player].Activate = bNewValue;

				if (bNewValue)
				{
					await EW._CP_api.SetClientCookie(player.SteamID.ToString(), "EW_Use_Priority", "1");
					UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Use_Priority"]} {EW.g_Scheme.color_enabled}{Strlocalizer["All.Enabled"]}", bConsole);
				}
				else
				{
					await EW._CP_api.SetClientCookie(player.SteamID.ToString(), "EW_Use_Priority", "0");
					UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Use_Priority"]} {EW.g_Scheme.color_disabled}{Strlocalizer["All.Disabled"]}", bConsole);
				}
			}catch (Exception ex) { Console.WriteLine(ex.ToString()); }
		}

		//Eban Start
		[ConsoleCommand("ew_ban", "Allows the admin to restrict items for the player")]
		[RequiresPermissions("@css/ew_ban")]
		[CommandHelper(minArgs: 1, usage: "<#userid|name> [<time>] [<reason>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
		public void OnEWBan(CCSPlayerController? admin, CommandInfo command)
		{
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			(List<CCSPlayerController> players, string targetname) = Find(admin, command, 1, true, true, MultipleFlags.NORMAL, false);

			OfflineBan target = null;

			if (players.Count > 0)
			{
				CCSPlayerController targetOnline = players.Single();

				if (!AdminManager.CanPlayerTarget(admin, targetOnline))
				{
					UI.EWReplyInfo(admin, "Reply.You_cannot_target", bConsole);
					return;
				}

				if (!EW.CheckDictionary(targetOnline, EW.g_BannedPlayer))
				{
					UI.EWReplyInfo(admin, "Info.Error", bConsole, "Player not found in dictionary");
					return;
				}

				if (EW.g_BannedPlayer[targetOnline].bBanned)
				{
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(targetOnline)} {Strlocalizer["Reply.Eban.Has_a_Restrict"]}", bConsole);
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

			UI.EWChatAdmin("Chat.Admin.Restricted", EW.g_Scheme.color_warning, UI.PlayerInfo(admin), EW.g_Scheme.color_disabled, target.Online ? UI.PlayerInfo(target.Player) : UI.PlayerInfo(target.Name,target.SteamID));
			UI.EWChatAdmin("Chat.Admin.Reason", EW.g_Scheme.color_warning, reason);
			Server.NextFrame(async () =>
			{
				EbanPlayer ebanPlayer = target.Online ? EW.g_BannedPlayer[target.Player] : new EbanPlayer();
				if (await ebanPlayer.SetBan(admin != null ? admin.PlayerName : "Console", admin != null ? EW.ConvertSteamID64ToSteamID(admin.SteamID.ToString()) : "SERVER", target.Name, target.SteamID, time, reason))
				{
					Server.NextFrame(() =>
					{
						if (admin != null && admin.IsValid) UI.EWReplyInfo(admin, "Reply.Eban.Ban.Success", bConsole); //admin.PrintToChat("Success");
						UI.EWSysInfo("Reply.Eban.Ban.Success", 6);
					});
				}
				else
				{
					Server.NextFrame(() =>
					{
						if (admin != null && admin.IsValid) UI.EWReplyInfo(admin, "Reply.Eban.Ban.Failed", bConsole); //admin.PrintToChat("Failed");
						UI.EWSysInfo("Reply.Eban.Ban.Failed", 15);
					});
				}
			});
		}

		[ConsoleCommand("ew_unban", "Allows the admin to remove the item restriction for a player")]
		[RequiresPermissions("@css/ew_unban")]
		[CommandHelper(minArgs: 1, usage: "<#userid|name> [<reason>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
		public void OnEWUnBan(CCSPlayerController? admin, CommandInfo command)
		{
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			(List<CCSPlayerController> players, string targetname) = Find(admin, command, 1, true, true, MultipleFlags.NORMAL, false);

			EbanPlayer target = new EbanPlayer();
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
				if (!EW.CheckDictionary(targetController, EW.g_BannedPlayer))
				{
					UI.EWReplyInfo(admin, "Info.Error", bConsole, "Player not found in dictionary");
					return;
				}
				target.bBanned = EW.g_BannedPlayer[targetController].bBanned;
				target.sAdminName = EW.g_BannedPlayer[targetController].sAdminSteamID;
				target.sAdminSteamID = EW.g_BannedPlayer[targetController].sAdminSteamID;
				target.iDuration = EW.g_BannedPlayer[targetController].iDuration;
				target.iTimeStamp_Issued = EW.g_BannedPlayer[targetController].iTimeStamp_Issued;
				target.sReason = EW.g_BannedPlayer[targetController].sReason;
				target.sClientName = targetController.PlayerName;
				target.sClientSteamID = EW.ConvertSteamID64ToSteamID(targetController.SteamID.ToString());
			}

			string reason = command.GetArg(2);
			if (string.IsNullOrEmpty(reason)) reason = Cvar.UnBanReason;

			Server.NextFrame(async () =>
			{
				if (!bOnline && sTarget.ToLower().StartsWith("steam_"))
				{
					target = await EbanDB.GetBan(sTarget, EW.g_Scheme.server_name);
				}

				if (target == null)
				{
					UI.EWReplyInfo(admin, "Reply.No_matching_client", bConsole);
					return;
				}

				if (!target.bBanned)
				{
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(target.sClientName,target.sClientSteamID)} {Strlocalizer["Reply.Eban.Can_pickup"]}", bConsole);
					return;
				}

				if (target.iDuration == 0 && !AdminManager.PlayerHasPermissions(admin, "@css/ew_unban_perm"))
				{
					UI.EWReplyInfo(admin, "Reply.Eban.Access.UnPermanent", bConsole);
					return;
				}

				if (admin != null && target.sAdminSteamID.CompareTo(EW.ConvertSteamID64ToSteamID(admin.SteamID.ToString())) != 0 && !AdminManager.PlayerHasPermissions(admin, "@css/ew_unban_other"))
				{
					UI.EWReplyInfo(admin, "Reply.Eban.Access.Other", bConsole);
					return;
				}

				Server.NextFrame(() =>
				{
					UI.EWChatAdmin("Chat.Admin.Unrestricted", EW.g_Scheme.color_warning, UI.PlayerInfo(admin), EW.g_Scheme.color_enabled, UI.PlayerInfo(target.sClientName, target.sClientSteamID));
					UI.EWChatAdmin("Chat.Admin.Reason", EW.g_Scheme.color_warning, reason);
				});

				Server.NextFrame(async () =>
				{
					if (await target.UnBan(admin != null ? admin.PlayerName : "Console", admin != null ? EW.ConvertSteamID64ToSteamID(admin.SteamID.ToString()) : "SERVER", target.sClientSteamID, reason))
					{
						Server.NextFrame(() =>
						{
							if (admin != null && admin.IsValid) UI.EWReplyInfo(admin, "Reply.Eban.UnBan.Success", bConsole); //admin.PrintToChat("Success");
							UI.EWSysInfo("Reply.Eban.UnBan.Success", 6);
						});
					}
					else
					{
						Server.NextFrame(() =>
						{
							if (admin != null && admin.IsValid) UI.EWReplyInfo(admin, "Reply.Eban.UnBan.Failed", bConsole); //admin.PrintToChat("Failed");
							UI.EWSysInfo("Reply.Eban.UnBan.Failed", 15);
						});
					}
				});
			});
		}

		[ConsoleCommand("ew_status", "Allows the player to view the restrictions")]
		[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_ONLY)]
		public void OnEWStatus(CCSPlayerController? player, CommandInfo command)
		{
			if (player != null && !player.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			CCSPlayerController target = player;
			if (command.ArgCount > 1)
			{
				(List<CCSPlayerController> players, string targetname) = Find(player, command, 1, true, false, MultipleFlags.NORMAL);

				if (players.Count == 0) return;

				target = players.Single();
			}
			if (!EW.CheckDictionary(target, EW.g_BannedPlayer))
			{
				UI.EWReplyInfo(player, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}
			if (EW.g_BannedPlayer[target].bBanned)
			{
				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(target)} {Strlocalizer["Reply.Eban.Has_a_Restrict"]}", bConsole);

				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Admin"]}: {EW.g_Scheme.color_name}{UI.PlayerInfo(EW.g_BannedPlayer[target].sAdminName, EW.g_BannedPlayer[target].sAdminSteamID)}", bConsole);

				switch(EW.g_BannedPlayer[target].iDuration)
				{
					case -1: UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_enabled}{Strlocalizer["Reply.Eban.Temporary"]}", bConsole); break;
					case 0: UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_disabled}{Strlocalizer["Reply.Eban.Permanently"]}", bConsole); break;
					default: UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_disabled}{EW.g_BannedPlayer[target].iDuration} {Strlocalizer["Reply.Eban.Minutes"]}", bConsole); break;
				}

				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Eban.Expires"]}: {EW.g_Scheme.color_disabled}{DateTimeOffset.FromUnixTimeSeconds(EW.g_BannedPlayer[target].iTimeStamp_Issued)}", bConsole);

				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Eban.Reason"]}: {EW.g_Scheme.color_disabled}{EW.g_BannedPlayer[target].sReason}", bConsole);
			} else
			{
				UI.ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(target)} {Strlocalizer["Reply.Eban.Can_pickup"]}", bConsole);
			}
		}
		[ConsoleCommand("ew_banlist", "Displays a list of restrictions")]
		[RequiresPermissions("@css/ew_ban")]
		[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
		public void OnEWBanList(CCSPlayerController? admin, CommandInfo command)
		{
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			UI.EWReplyInfo(admin, "Reply.Eban.List", bConsole);
			int iCount = 0;
			Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(target =>
			{
				if (EW.CheckDictionary(target, EW.g_BannedPlayer) && EW.g_BannedPlayer[target].bBanned)
				{
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}***{Strlocalizer["Reply.Player"]} {UI.PlayerInfo(target)}{EW.g_Scheme.color_warning}***", bConsole);
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Admin"]} {EW.g_Scheme.color_name}{UI.PlayerInfo(EW.g_BannedPlayer[target].sAdminName, EW.g_BannedPlayer[target].sAdminSteamID)}", bConsole);
					switch (EW.g_BannedPlayer[target].iDuration)
					{
						case -1: UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_enabled}{Strlocalizer["Reply.Eban.Temporary"]}", bConsole); break;
						case 0: UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_disabled}{Strlocalizer["Reply.Eban.Permanently"]}", bConsole); break;
						default: UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Duration"]}: {EW.g_Scheme.color_disabled}{EW.g_BannedPlayer[target].iDuration} {Strlocalizer["Reply.Eban.Minutes"]}", bConsole); break;
					}
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Expires"]}: {EW.g_Scheme.color_disabled}{DateTimeOffset.FromUnixTimeSeconds(EW.g_BannedPlayer[target].iTimeStamp_Issued)}", bConsole);

					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|{Strlocalizer["Reply.Eban.Reason"]}: {EW.g_Scheme.color_disabled}{EW.g_BannedPlayer[target].sReason}", bConsole);
					
					UI.ReplyToCommand(admin, $"{EW.g_Scheme.color_warning}|__________________________________________", bConsole);
					iCount++;
				}
			});
			if (iCount == 0) UI.EWReplyInfo(admin, "Reply.Eban.NoPlayers", bConsole);
		}
		//Eban End

		[ConsoleCommand("ew_transfer", "Allows the admin to transfer items")]
		[RequiresPermissions("@css/ew_transfer")]
		[CommandHelper(minArgs: 2, usage: "<owner>/$<itemname> <receiver>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
		public void OnEWTransfer(CCSPlayerController? admin, CommandInfo command)
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
					if (ItemTest.Name.ToLower().Contains(sItemName) || ItemTest.ShortName.ToLower().Contains(sItemName))
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
				(List<CCSPlayerController> players, string targetname) = Find(admin, command, 1, true, true, MultipleFlags.IGNORE_DEAD_PLAYERS);
				if (players.Count == 0) return;
				target = players.Single();
			}

			if (target != null && !AdminManager.CanPlayerTarget(admin, target))
			{
				UI.EWReplyInfo(admin, "Reply.You_cannot_target", bConsole);
				return;
			}

			(List<CCSPlayerController> players1, string targetname1) = Find(admin, command, 2, true, false, MultipleFlags.IGNORE_DEAD_PLAYERS);
			if (players1.Count == 0) return;
			CCSPlayerController receiver = players1.Single();

			if (!EW.CheckDictionary(receiver, EW.g_BannedPlayer))
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

			if (EW.g_BannedPlayer[receiver].bBanned)
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
		[RequiresPermissions("@css/ew_spawn")]
		[CommandHelper(minArgs: 2, usage: "<itemname> <receiver> [<strip>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
		public void OnEWSpawn(CCSPlayerController? admin, CommandInfo command)
		{
			if (admin != null && !admin.IsValid) return;
			bool bConsole = command.CallingContext == CommandCallingContext.Console;

			string sItemName = command.GetArg(1);

			if(string.IsNullOrEmpty(sItemName))
			{
				UI.EWReplyInfo(admin, "Reply.Spawn.BadItemName", bConsole);
				return;
			}

			(List<CCSPlayerController> players, string targetname1) = Find(admin, command, 2, true, false, MultipleFlags.IGNORE_DEAD_PLAYERS);
			if (players.Count == 0) return;
			CCSPlayerController receiver = players.Single();

			if (!EW.CheckDictionary(receiver, EW.g_BannedPlayer))
			{
				UI.EWReplyInfo(admin, "Info.Error", bConsole, "Player not found in dictionary");
				return;
			}

			if (EW.g_BannedPlayer[receiver].bBanned)
			{
				UI.EWReplyInfo(admin, "Reply.Spawn.ReceiverHasBan", bConsole);
				return;
			}

			bool bStrip = false;
			string sStrip = command.GetArg(3);
			if (!string.IsNullOrEmpty(sStrip)) bStrip = sStrip.ToLower().Contains("true") || sStrip.CompareTo("1") == 0;

			SpawnItem.Spawn(admin, receiver, sItemName, bStrip, bConsole);
		}

		[ConsoleCommand("ew_list", "Shows a list of players including those who have disconnected")]
		[RequiresPermissions("@css/ew_ban")]
		[CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
		public void OnEWList(CCSPlayerController? admin, CommandInfo command)
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
