using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using EntWatchSharp.Items;
using System.Globalization;

namespace EntWatchSharp.Helpers
{
	static class UI
	{
		public static void EWChatActivity(string sMessage, string sColor, Item ItemTest, CCSPlayerController player, Ability AbilityTest = null)
		{
			string[] sPlayerInfoFormat = PlayerInfoFormat(player);
			using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
			{
				PrintToConsole($"{sPlayerInfoFormat[3]} {sColor}{EntWatchSharp.Strlocalizer[sMessage]} {ItemTest.Color}{ItemTest.Name}{((AbilityTest != null && !string.IsNullOrEmpty(AbilityTest.Name)) ? $" ({AbilityTest.Name})" : "")}");
			}

			Task.Run(() =>
			{
				LogManager.ItemAction(sMessage, sPlayerInfoFormat[3], $"{ItemTest.Name}{((AbilityTest != null && !string.IsNullOrEmpty(AbilityTest.Name)) ? $" ({AbilityTest.Name})" : "")}");
			});
			
			if (!(AbilityTest == null || ItemTest.Chat || AbilityTest.Chat_Uses)) return;

			foreach (var pair in EW.g_EWPlayer)
			{
				Server.NextFrame(() =>
				{
					if (!(pair.Key is { IsValid: true, IsBot: false, IsHLTV: false }) || !EW.g_EWPlayer.ContainsKey(pair.Key)) return;

					if (Cvar.TeamOnly && pair.Key.TeamNum > 1 && ItemTest.Team != pair.Key.TeamNum && (!AdminManager.PlayerHasPermissions(pair.Key, "@css/ew_chat") || Cvar.AdminChat == 2 || (Cvar.AdminChat == 1 && AbilityTest != null))) return;

					using (new WithTemporaryCulture(pair.Key.GetLanguage()))
					{
						pair.Key.PrintToChat(EWChatMessage($"{PlayerInfo(pair.Key, sPlayerInfoFormat)} {sColor}{EntWatchSharp.Strlocalizer[sMessage]} {ItemTest.Color}{ItemTest.Name}{((AbilityTest != null && !string.IsNullOrEmpty(AbilityTest.Name)) ? $" ({AbilityTest.Name})" : "")}"));
					}
				});
			}
		}
		public static void EWChatAdminBan(string[] sPIF_admin, string[] sPIF_player, string sReason, bool bAction)
		{
			Server.NextFrame(() =>
			{
				using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
				{
					PrintToConsole(EntWatchSharp.Strlocalizer[bAction ? "Chat.Admin.Restricted" : "Chat.Admin.Unrestricted", EW.g_Scheme.color_warning, sPIF_admin[3], bAction ? EW.g_Scheme.color_disabled : EW.g_Scheme.color_enabled, sPIF_player[3]], 2);
					PrintToConsole(EntWatchSharp.Strlocalizer["Chat.Admin.Reason", EW.g_Scheme.color_warning, sReason], 2);
				}

				Task.Run(() =>
				{
					LogManager.AdminAction(bAction ? "Chat.Admin.Restricted" : "Chat.Admin.Unrestricted", EW.g_Scheme.color_warning, sPIF_admin[3], bAction ? EW.g_Scheme.color_disabled : EW.g_Scheme.color_enabled, sPIF_player[3]);
					LogManager.AdminAction("Chat.Admin.Reason", EW.g_Scheme.color_warning, sReason);
				});

				foreach (var pair in EW.g_EWPlayer)
				{
					Server.NextFrame(() =>
					{
						if (!(pair.Key is { IsValid: true, IsBot: false, IsHLTV: false })) return;
						using (new WithTemporaryCulture(pair.Key.GetLanguage()))
						{
							pair.Key.PrintToChat(EWChatMessage(EntWatchSharp.Strlocalizer[bAction ? "Chat.Admin.Restricted" : "Chat.Admin.Unrestricted", EW.g_Scheme.color_warning, PlayerInfo(pair.Key, sPIF_admin), bAction ? EW.g_Scheme.color_disabled : EW.g_Scheme.color_enabled, PlayerInfo(pair.Key, sPIF_player)]));
							pair.Key.PrintToChat(EWChatMessage(EntWatchSharp.Strlocalizer["Chat.Admin.Reason", EW.g_Scheme.color_warning, sReason]));
						}
					});
				}
			});
		}
		public static void EWChatAdminSpawn(string[] sPIF_admin, string[] sPIF_receiver, string sItem)
		{
			Server.NextFrame(() =>
			{
				using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
				{
					PrintToConsole(EntWatchSharp.Strlocalizer["Reply.Spawn.Notify", sPIF_admin[3], sItem, sPIF_receiver[3]], 2);
				}

				Task.Run(() =>
				{
					LogManager.AdminAction("Reply.Spawn.Notify", sPIF_admin[3], sItem, sPIF_receiver[3]);
				});

				foreach (var pair in EW.g_EWPlayer)
				{
					Server.NextFrame(() =>
					{
						if (!(pair.Key is { IsValid: true, IsBot: false, IsHLTV: false })) return;
						using (new WithTemporaryCulture(pair.Key.GetLanguage()))
						{
							pair.Key.PrintToChat(EWChatMessage(EntWatchSharp.Strlocalizer["Reply.Spawn.Notify", PlayerInfo(pair.Key, sPIF_admin), sItem, PlayerInfo(pair.Key, sPIF_receiver)]));
						}
					});
				}
			});
		}
		public static void EWChatAdminTransfer(string[] sPIF_admin, string[] sPIF_receiver, string sItem, string[] sPIF_target)
		{
			Server.NextFrame(() =>
			{
				using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
				{
					PrintToConsole(EntWatchSharp.Strlocalizer["Reply.Transfer.Notify", sPIF_admin[3], sItem, sPIF_target[3], sPIF_receiver[3]], 2);
				}

				Task.Run(() =>
				{
					LogManager.AdminAction("Reply.Transfer.Notify", sPIF_admin[3], sItem, sPIF_target[3], sPIF_receiver[3]);
				});

				foreach (var pair in EW.g_EWPlayer)
				{
					Server.NextFrame(() =>
					{
						if (!(pair.Key is { IsValid: true, IsBot: false, IsHLTV: false })) return;
						using (new WithTemporaryCulture(pair.Key.GetLanguage()))
						{
							pair.Key.PrintToChat(EWChatMessage(EntWatchSharp.Strlocalizer["Reply.Transfer.Notify", PlayerInfo(pair.Key, sPIF_admin), sItem, PlayerInfo(pair.Key, sPIF_target), PlayerInfo(pair.Key, sPIF_receiver)]));
						}
					});
				}
			});
		}

		public static void EWSysInfo(string sMessage, int iColor = 15, params object[] arg)
		{
			using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
			{
				PrintToConsole(EntWatchSharp.Strlocalizer[sMessage, arg], iColor);
			}
			Task.Run(() =>
			{
				LogManager.SystemAction(sMessage, arg);
			});
		}

		public static void EWReplyInfo(CCSPlayerController player, string sMessage, bool bConsole, params object[] arg)
		{
			using (new WithTemporaryCulture(player.GetLanguage()))
			{
				ReplyToCommand(player, $"{EW.g_Scheme.color_warning}{EntWatchSharp.Strlocalizer[sMessage, arg]}", bConsole);
			}
		}

		public static void CvarChangeNotify(string sCvarName, string sCvarValue, bool bClientNotify)
		{
			using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
			{
				PrintToConsole(EntWatchSharp.Strlocalizer["Cvar.Notify", sCvarName, sCvarValue], 3);
			}

			Task.Run(() =>
			{
				LogManager.CvarAction(sCvarName, sCvarValue);
			});

			if (bClientNotify)
			{
				foreach (var pair in EW.g_EWPlayer)
				{
					Server.NextFrame(() =>
					{
						if (!(pair.Key is { IsValid: true, IsBot: false, IsHLTV: false })) return;
						using (new WithTemporaryCulture(pair.Key.GetLanguage()))
						{
							pair.Key.PrintToChat(EWChatMessage($"{EW.g_Scheme.color_warning}{EntWatchSharp.Strlocalizer["Cvar.Notify", sCvarName, sCvarValue]}"));
						}
					});
				}
			}
		}

		public static void PrintToConsole(string sMessage, int iColor = 1)
		{
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("[");
			Console.ForegroundColor = (ConsoleColor)6;
			Console.Write("EntWatch");
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("] ");
			Console.ForegroundColor = (ConsoleColor)iColor;
			Console.WriteLine(ReplaceColorTags(sMessage, false));
			Console.ResetColor();
			/* Colors:
				* 0 - No color		1 - White		2 - Red-Orange		3 - Orange
				* 4 - Yellow		5 - Dark Green	6 - Green			7 - Light Green
				* 8 - Cyan			9 - Sky			10 - Light Blue		11 - Blue
				* 12 - Violet		13 - Pink		14 - Light Red		15 - Red */
		}

		public static void ReplyToCommand(CCSPlayerController player, string sMessage, bool bConsole = false)
		{
			Server.NextFrame(() =>
			{
				if (player is { IsValid: true, IsBot: false, IsHLTV: false })
				{
					using (new WithTemporaryCulture(player.GetLanguage()))
					{
						if(!bConsole) player.PrintToChat(EWChatMessage(sMessage));
						else player.PrintToConsole(EWChatMessage(sMessage));
					}
				}
				else
				{
					using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
					{
						PrintToConsole(sMessage, 13);
					}
				}
			});
		}

		public static string ReplaceSpecial(string input)
		{
			input = input.Replace("{", "[");
			input = input.Replace("}", "]");

			return input;
		}
		public static string ReplaceColorTags(string input, bool bChat = true)
		{
			for (var i = 0; i < colorPatterns.Length; i++)
				input = input.Replace(colorPatterns[i], bChat ? colorReplacements[i] : "");

			return input;
		}

		static string EWChatMessage(string sMessage)
		{
			sMessage = $"{Tag()}{sMessage}";
			sMessage = ReplaceColorTags(sMessage);
			return sMessage;
		}

		public static string Tag()
		{
			return $" {EW.g_Scheme.color_tag} [EntWatch] ";
		}
#nullable enable
		public static string PlayerInfo(CCSPlayerController? player, string[] sPlayerInfoFormat)
#nullable disable
		{
			if (player != null)
			{
				if (EW.g_EWPlayer[player].PFormatPlayer < 0 || EW.g_EWPlayer[player].PFormatPlayer > 3) return sPlayerInfoFormat[Cvar.PlayerFormat];
				return sPlayerInfoFormat[EW.g_EWPlayer[player].PFormatPlayer];
			}
			return sPlayerInfoFormat[3];
		}
		public static string[] PlayerInfoFormat(CCSPlayerController player)
		{
			if (player != null)
			{
				string[] sResult = new string[4];
				sResult[0] = $"{EW.g_Scheme.color_name}{player.PlayerName}{EW.g_Scheme.color_warning}";
				sResult[1] = $"{sResult[0]}[{EW.g_Scheme.color_steamid}#{player.UserId}{EW.g_Scheme.color_warning}]";
				sResult[2] = $"{sResult[0]}[{EW.g_Scheme.color_steamid}#{EW.ConvertSteamID64ToSteamID(player.SteamID.ToString())}{EW.g_Scheme.color_warning}]";
				sResult[3] = $"{sResult[0]}[{EW.g_Scheme.color_steamid}#{player.UserId}{EW.g_Scheme.color_warning}|{EW.g_Scheme.color_steamid}#{EW.ConvertSteamID64ToSteamID(player.SteamID.ToString())}{EW.g_Scheme.color_warning}]";
				return sResult;
			}
			return PlayerInfoFormat("Console", "Server");
		}
		public static string[] PlayerInfoFormat(string sName, string sSteamID)
		{
			string[] sResult = new string[4];
			sResult[0] = $"{EW.g_Scheme.color_name}{sName}{EW.g_Scheme.color_warning}";
			sResult[1] = sResult[0];
			sResult[2] = $"{EW.g_Scheme.color_name}{sName}{EW.g_Scheme.color_warning}[{EW.g_Scheme.color_steamid}{sSteamID}{EW.g_Scheme.color_warning}]";
			sResult[3] = sResult[2];
			return sResult;
		}
		readonly static string[] colorPatterns =
		[
			"{default}", "{darkred}", "{purple}", "{green}", "{lightgreen}", "{lime}", "{red}", "{grey}", "{team}", "{red2}",
			"{olive}", "{a}", "{lightblue}", "{blue}", "{d}", "{pink}", "{darkorange}", "{orange}", "{darkblue}", "{gold}",
			"{white}", "{yellow}", "{magenta}", "{silver}", "{bluegrey}", "{lightred}", "{cyan}", "{gray}", "{lightyellow}", 
		];
		readonly static string[] colorReplacements =
		[
			"\x01", "\x02", "\x03", "\x04", "\x05", "\x06", "\x07", "\x08", "\x03", "\x0F",
			"\x06", "\x0A", "\x0B", "\x0C", "\x0D", "\x0E", "\x0F", "\x10", "\x0C", "\x10",
			"\x01", "\x09", "\x0E", "\x0A", "\x0D", "\x0F", "\x03", "\x08", "\x06"
		];
	}
}
