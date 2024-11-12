using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core.Translations;
using System.Globalization;
using CounterStrikeSharp.API.Modules.Admin;
using EntWatchSharp.Items;
using CounterStrikeSharp.API.Modules.Entities;

namespace EntWatchSharp.Helpers
{
    static class UI
    {
		public static void EWChatActivity(string sMessage, string sColor, Item ItemTest, CCSPlayerController player, Ability AbilityTest = null)
		{
			using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
			{
				PrintToConsole($"{PlayerInfo(player)} {sColor}{EntWatchSharp.Strlocalizer[sMessage]} {ItemTest.Color}{ItemTest.Name}{((AbilityTest != null && !string.IsNullOrEmpty(AbilityTest.Name)) ? $" ({AbilityTest.Name})" : "")}");
			}

            LogManager.ItemAction(sMessage, PlayerInfo(player), $"{ItemTest.Name}{((AbilityTest != null && !string.IsNullOrEmpty(AbilityTest.Name)) ? $" ({AbilityTest.Name})" : "")}");

			if (!(AbilityTest == null || ItemTest.Chat || AbilityTest.Chat_Uses)) return;

			Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(pl =>
			{
				Server.NextFrame(() =>
				{
					if (!player.IsValid) return;

					if (Cvar.TeamOnly && player.TeamNum > 1 && ItemTest.Team != player.TeamNum && (!AdminManager.PlayerHasPermissions(player, "@css/ew_chat") || Cvar.AdminChat == 2 || (Cvar.AdminChat == 1 && AbilityTest != null))) return;

					using (new WithTemporaryCulture(pl.GetLanguage()))
					{
						pl.PrintToChat(EWChatMessage($"{PlayerInfo(player)} {sColor}{EntWatchSharp.Strlocalizer[sMessage]} {ItemTest.Color}{ItemTest.Name}{((AbilityTest != null && !string.IsNullOrEmpty(AbilityTest.Name)) ? $" ({AbilityTest.Name})" : "")}"));
					}
				});
			});
		}

        public static void EWChatAdmin(string sMessage, params object[] arg)
        {
            using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
            {
                PrintToConsole(EntWatchSharp.Strlocalizer[sMessage, arg], 2);
            }

            LogManager.AdminAction(sMessage, arg);


			Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(pl =>
            {
                Server.NextFrame(() =>
                {
                    using (new WithTemporaryCulture(pl.GetLanguage()))
                    {
                        pl.PrintToChat(EWChatMessage(EntWatchSharp.Strlocalizer[sMessage, arg]));
                    }
                });
            });
        }

        public static void EWSysInfo(string sMessage, int iColor = 15, params object[] arg)
        {
            using (new WithTemporaryCulture(CultureInfo.GetCultureInfo(CoreConfig.ServerLanguage)))
            {
                PrintToConsole(EntWatchSharp.Strlocalizer[sMessage, arg], iColor);
            }
			LogManager.SystemAction(sMessage, arg);
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

            LogManager.CvarAction(sCvarName, sCvarValue);

			if (bClientNotify)
            {
                Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false }).ToList().ForEach(pl =>
                {
                    Server.NextFrame(() =>
                    {
                        using (new WithTemporaryCulture(pl.GetLanguage()))
                        {
                            pl.PrintToChat(EWChatMessage($"{EW.g_Scheme.color_warning}{EntWatchSharp.Strlocalizer["Cvar.Notify", sCvarName, sCvarValue]}"));
                        }
                    });
                });
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
        public static string PlayerInfo(CCSPlayerController player)
        {
			return player != null ? $"{EW.g_Scheme.color_name}{player.PlayerName}{EW.g_Scheme.color_warning}[{EW.g_Scheme.color_steamid}#{player.UserId}{EW.g_Scheme.color_warning}|{EW.g_Scheme.color_steamid}#{EW.ConvertSteamID64ToSteamID(player.SteamID.ToString())}{EW.g_Scheme.color_warning}]" : PlayerInfo("Console", "Server");
		}
        public static string PlayerInfo(string sName, string sSteamID)
        {
            return $"{EW.g_Scheme.color_name}{sName} {EW.g_Scheme.color_warning}[{EW.g_Scheme.color_steamid}{sSteamID}{EW.g_Scheme.color_warning}]";
        }
        static string[] colorPatterns =
        {
            "{default}", "{darkred}", "{purple}", "{green}", "{lightgreen}", "{lime}", "{red}", "{grey}",
            "{olive}", "{a}", "{lightblue}", "{blue}", "{d}", "{pink}", "{darkorange}", "{orange}",
            "{white}", "{yellow}", "{magenta}", "{silver}", "{bluegrey}", "{lightred}", "{cyan}", "{gray}"
        };
        static string[] colorReplacements =
        {
            "\x01", "\x02", "\x03", "\x04", "\x05", "\x06", "\x07", "\x08",
            "\x09", "\x0A", "\x0B", "\x0C", "\x0D", "\x0E", "\x0F", "\x10",
            "\x01", "\x09", "\x0E", "\x0A", "\x0D", "\x0F", "\x03", "\x08"
        };
    }
}
