using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;
using EntWatchSharp.Items;

namespace EntWatchSharp.Modules
{
    class UHudItem
    {
        bool bNull = false;
        bool bSeparator = false;
        string sItemNameText = "";
        string sItemColorClass = "";
        byte iTextSize = 99; //small - 0, normal - 1, big - 2, large - 3
        string sCooldownText = "";
        string sCooldownColorClass = "";
        string sNicknameText = "";
        string sNicknameColorClass = "";
        string sSteamIDText = "";
        string sSteamIDColorClass = "";
        byte iProgressValue = 0;

        readonly string __Cache_ItemName;
        readonly string __Cache_Cooldown;
        readonly string __Cache_Nickname;
        readonly string __Cache_Steam;
        readonly string __Cache_ProgressBar;
        readonly string __Cache_Button;
        readonly string __Cache_ItemName_Text;
        readonly string __Cache_Cooldown_Text;
        readonly string __Cache_Nickname_Text;
        readonly string __Cache_Steam_Text;

        public UHudItem(byte iNum)
        {
            string sNum = iNum.ToString("D2");
            __Cache_ItemName = $"entwatch-itemname-{sNum}";
            __Cache_Cooldown = $"entwatch-cd-{sNum}";
            __Cache_Nickname = $"entwatch-nickname-{sNum}";
            __Cache_Steam = $"entwatch-steam-{sNum}";
            __Cache_ProgressBar = $"entwatch-pb-{sNum}";
            __Cache_Button = $"entwatch-btn-{sNum}";
            __Cache_ItemName_Text = $"entwatch-itemname-text-{sNum}";
            __Cache_Cooldown_Text = $"entwatch-cd-text-{sNum}";
            __Cache_Nickname_Text = $"entwatch-nickname-text-{sNum}";
            __Cache_Steam_Text = $"entwatch-steam-text-{sNum}";
        }

        public void SetButtonSeparator(CCSCustomHudLayout eHud, CCSPlayerController HudPlayer)
        {
            if (bNull)
            {
                eHud.SetHasClassForPlayer(HudPlayer, __Cache_Button, "null", false);
                bNull = false;
            }
            if (!bSeparator)
            {
                eHud.SetHasClassForPlayer(HudPlayer, __Cache_Button, "separator", true);
                ChangeValue(eHud, HudPlayer, "", sItemColorClass, iTextSize, "", sCooldownColorClass, "", sNicknameColorClass, "", sSteamIDColorClass, 0);
                bSeparator = true;
            }
        }

        public void SetButtonNull(CCSCustomHudLayout eHud, CCSPlayerController HudPlayer)
        {
            if (bSeparator)
            {
                eHud.SetHasClassForPlayer(HudPlayer, __Cache_Button, "separator", false);
                bSeparator = false;
            }
            if (!bNull)
            {
                eHud.SetHasClassForPlayer(HudPlayer, __Cache_Button, "null", true);
                ChangeValue(eHud, HudPlayer, "", sItemColorClass, iTextSize, "", sCooldownColorClass, "", sNicknameColorClass, "", sSteamIDColorClass, 0);
                bNull = true;
            }
        }

        public void SetButtonItem(CCSCustomHudLayout eHud, CCSPlayerController HudPlayer)
        {
            if (bSeparator)
            {
                eHud.SetHasClassForPlayer(HudPlayer, __Cache_Button, "separator", false);
                bSeparator = false;
            }
            if (bNull)
            {
                eHud.SetHasClassForPlayer(HudPlayer, __Cache_Button, "null", false);
                bNull = false;
            }
        }

        public void ChangeValue(CCSCustomHudLayout eHud, CCSPlayerController HudPlayer, string _ItemNameText, string _ItemColorClass, byte _TextSize, string _CooldownText, string _CooldownColorClass, string _NicknameText, string _NicknameColorClass, string _SteamIDText, string _SteamIDColorClass, byte _ProgressValue)
        {
            if (!string.Equals(sItemNameText, _ItemNameText, StringComparison.Ordinal))
            {
                eHud.SetDialogVariableStringForPlayer(HudPlayer, __Cache_ItemName, __Cache_ItemName_Text, _ItemNameText);
                sItemNameText = _ItemNameText;
            }
            if (!string.Equals(sCooldownText, _CooldownText, StringComparison.Ordinal))
            {
                eHud.SetDialogVariableStringForPlayer(HudPlayer, __Cache_Cooldown, __Cache_Cooldown_Text, _CooldownText);
                sCooldownText = _CooldownText;
            }
            if (!string.Equals(sNicknameText, _NicknameText, StringComparison.Ordinal))
            {
                eHud.SetDialogVariableStringForPlayer(HudPlayer, __Cache_Nickname, __Cache_Nickname_Text, _NicknameText);
                sNicknameText = _NicknameText;
            }
            if (!string.Equals(sSteamIDText, _SteamIDText, StringComparison.Ordinal))
            {
                eHud.SetDialogVariableStringForPlayer(HudPlayer, __Cache_Steam, __Cache_Steam_Text, _SteamIDText);
                sSteamIDText = _SteamIDText;
            }

            bool UpdateColorClasses(string sOldClass, string sNewClass, string sCacheName)
            {
                if (!string.Equals(sOldClass, sNewClass, StringComparison.Ordinal))
                {
                    if (!string.IsNullOrEmpty(sOldClass)) eHud.SetHasClassForPlayer(HudPlayer, sCacheName, sOldClass, false);
                    eHud.SetHasClassForPlayer(HudPlayer, sCacheName, sNewClass, true);
                    return true;
                }
                return false;
            }

            UpdateColorClasses(sItemColorClass, _ItemColorClass, __Cache_ItemName);
            if (UpdateColorClasses($"bg-{sItemColorClass}", $"bg-{_ItemColorClass}", __Cache_ProgressBar)) sItemColorClass = _ItemColorClass;
            if (UpdateColorClasses(sCooldownColorClass, _CooldownColorClass, __Cache_Cooldown)) sCooldownColorClass = _CooldownColorClass;
            if (UpdateColorClasses(sNicknameColorClass, _NicknameColorClass, __Cache_Nickname)) sNicknameColorClass = _NicknameColorClass;
            if (UpdateColorClasses(sSteamIDColorClass, _SteamIDColorClass, __Cache_Steam)) sSteamIDColorClass = _SteamIDColorClass;

            if (iTextSize != _TextSize)
            {
                void UpdateSizeClasses(int iTextSizeValue, bool status)
                {
                    var (sSizeSteamID, sSizeAll) = iTextSizeValue switch
                    {
                        0 => ("size-smallest", "size-small"),
                        1 => ("size-smallest", "size-normal"),
                        2 => ("size-smallest", "size-big"),
                        3 => ("size-smallest", "size-large"),
                        _ => ("size-smallest", "size-normal")
                    };

                    eHud.SetHasClassForPlayer(HudPlayer, __Cache_ItemName, sSizeAll, status);
                    eHud.SetHasClassForPlayer(HudPlayer, __Cache_Cooldown, sSizeAll, status);
                    eHud.SetHasClassForPlayer(HudPlayer, __Cache_Nickname, sSizeAll, status);
                    eHud.SetHasClassForPlayer(HudPlayer, __Cache_Steam, sSizeSteamID, status);
                    eHud.SetHasClassForPlayer(HudPlayer, "ew-panel", $"panel-{sSizeAll}", status);
                }

                UpdateSizeClasses(iTextSize, false);
                UpdateSizeClasses(_TextSize, true);

                iTextSize = _TextSize;
            }

            if (iProgressValue != _ProgressValue)
            {
                eHud.SetHasClassForPlayer(HudPlayer, __Cache_ProgressBar, $"pct-{iProgressValue}", false);
                eHud.SetHasClassForPlayer(HudPlayer, __Cache_ProgressBar, $"pct-{_ProgressValue}", true);
                iProgressValue = _ProgressValue;
            }
        }
    }

    class UHud
    {
        public bool bShow = true;
        public bool bCaptureEnabled = false;
        readonly UHudItem[] UHudArray = new UHudItem[15];
        public int iRefresh = 3;
        public byte iSize = 1;
        int iCurrentNumListH = 0;
        int iCurrentNumListZM = 0;
        double fNextUpdateList = EW.fGameTime - 3;
        public UHud()
        {
            for (byte i = 0; i < UHudArray.Length; i++) UHudArray[i] = new UHudItem((byte)(i + 1));
        }

        public void CaptureChange(CCSPlayerController HudPlayer, bool bEnabled)
        {
            if (EW.GetorCreateHudLayout() is { } hud && bShow)
            {
                bCaptureEnabled = bEnabled;
                if (bEnabled) hud.SetInputCaptureEnabled(HudPlayer, true);
                else hud.SetInputCaptureEnabled(HudPlayer, false);
            }
        }

        public void UpdateHUD(CCSPlayerController HudPlayer)
        {
            if (EW.GetorCreateHudLayout() is { } hud)
            {
                if (!bShow)
                {
                    hud.SetHasClassForPlayer(HudPlayer, "ew-panel", "ew-show", false);
                    hud.SetHasClassForPlayer(HudPlayer, "ew-panel", "ew-hide", true);
                    if (hud.IsInputCaptureEnabled(HudPlayer)) hud.SetInputCaptureEnabled(HudPlayer, false);
                }
                else
                {
                    List<Item> ListShowH = [];
                    List<Item> ListShowZM = [];
                    bool bAdminPermissions = AdminManager.PlayerHasPermissions(HudPlayer, "@css/ew_hud") && Cvar.AdminHud < 2;
                    foreach (Item ItemTest in EW.g_ItemList.ToList())
                    {
                        if (ItemTest.Owner != null)
                        {
                            if (ItemTest.Hud && (!Cvar.TeamOnly || HudPlayer.TeamNum < 2 || ItemTest.Team == HudPlayer.TeamNum || bAdminPermissions))
                            {
                                if (ItemTest.Team == 3) ListShowH.Add(ItemTest);
                                else if (ItemTest.Team == 2) ListShowZM.Add(ItemTest);
                            }
                        }
                    }

                    if (ListShowH.Count == 0 && ListShowZM.Count == 0)
                    {
                        hud.SetHasClassForPlayer(HudPlayer, "ew-panel", "ew-show", false);
                        hud.SetHasClassForPlayer(HudPlayer, "ew-panel", "ew-hide", true);
                        if (hud.IsInputCaptureEnabled(HudPlayer)) hud.SetInputCaptureEnabled(HudPlayer, false);
                        return;
                    }
                    hud.SetHasClassForPlayer(HudPlayer, "ew-panel", "ew-hide", false);
                    hud.SetHasClassForPlayer(HudPlayer, "ew-panel", "ew-show", true);

                    byte iCurrentNumHUD = 0;
                    bool bNextUpdateSync = true;
                    if (ListShowH.Count > 0)
                    {
                        byte iCountForList = 5;
                        if (ListShowZM.Count == 0) iCountForList = 13;
                        int iCountListH = (ListShowH.Count - 1) / iCountForList + 1;

                        if (fNextUpdateList <= EW.fGameTime)
                        {
                            iCurrentNumListH++;
                            fNextUpdateList = EW.fGameTime + iRefresh;
                            bNextUpdateSync = false;
                        }
                        if (iCurrentNumListH >= iCountListH) iCurrentNumListH = 0;

                        UHudArray[iCurrentNumHUD].SetButtonItem(hud, HudPlayer);
                        UHudArray[iCurrentNumHUD++].ChangeValue(hud, HudPlayer, "EntWatch Humans:", "color-team", iSize, "", "", "", "", "", "", 0);

                        for (int i = iCurrentNumListH * iCountForList; i < ListShowH.Count && i < (iCurrentNumListH + 1) * iCountForList; i++)
                        {
                            string sAbilityMessage = "";
                            (string, byte) ColorAbilityProgress = ("color-white", 0);
                            if (!Cvar.TeamOnly || HudPlayer.TeamNum < 2 || ListShowH[i].Team == HudPlayer.TeamNum || bAdminPermissions && Cvar.AdminHud == 0)
                            {
                                if (ListShowH[i].CheckDelay())
                                {
                                    int iAbilityCount = 0;
                                    foreach (Ability AbilityTest in ListShowH[i].AbilityList.ToList())
                                    {
                                        if (iAbilityCount > Cvar.DisplayAbility) break;
                                        if (!AbilityTest.Ignore)
                                        {
                                            iAbilityCount++;
                                            sAbilityMessage += $"|{AbilityTest.GetMessage()}";
                                            if (iAbilityCount == 1) ColorAbilityProgress = AbilityTest.GetColorAndProgress();
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(sAbilityMessage)) sAbilityMessage = sAbilityMessage[1..];
                                    if (iAbilityCount != 1) ColorAbilityProgress = ("color-white", 0);
                                }
                                else
                                {
                                    sAbilityMessage = $"-{Math.Round(ListShowH[i].fDelay - EW.fGameTime, 1)}";
                                    ColorAbilityProgress = ("color-blue", 0);
                                }
                            }
                            string sOwnerSteam = "";
                            if (ListShowH[i].Owner is { } client) sOwnerSteam = $"#{client.UserId}|#{EW.ConvertSteamID64ToSteamID(client.SteamID.ToString())}";

                            UHudArray[iCurrentNumHUD].SetButtonItem(hud, HudPlayer);
                            UHudArray[iCurrentNumHUD++].ChangeValue(hud, HudPlayer, ListShowH[i].ShortName, GetCSSClassColor(ListShowH[i].Color), iSize, sAbilityMessage, ColorAbilityProgress.Item1, $"{ListShowH[i].Owner?.PlayerName}", "color-white", sOwnerSteam, "color-grey", ColorAbilityProgress.Item2);
                        }
                        if (iCountListH > 1)
                        {
                            UHudArray[iCurrentNumHUD].SetButtonItem(hud, HudPlayer);
                            UHudArray[iCurrentNumHUD++].ChangeValue(hud, HudPlayer, "List:", "color-silver", iSize, $"[{iCurrentNumListH + 1}/{iCountListH}]", "color-silver", "", "", "", "", 0);
                        }
                    }

                    if (ListShowZM.Count > 0)
                    {
                        byte iCountForList = 5;
                        if (ListShowH.Count == 0) iCountForList = 13;
                        else UHudArray[iCurrentNumHUD++].SetButtonSeparator(hud, HudPlayer);

                        int iCountListZM = (ListShowZM.Count - 1) / iCountForList + 1;

                        if (!bNextUpdateSync || fNextUpdateList <= EW.fGameTime)
                        {
                            iCurrentNumListZM++;
                            if (bNextUpdateSync) fNextUpdateList = EW.fGameTime + iRefresh;
                        }
                        if (iCurrentNumListZM >= iCountListZM) iCurrentNumListZM = 0;

                        UHudArray[iCurrentNumHUD].SetButtonItem(hud, HudPlayer);
                        UHudArray[iCurrentNumHUD++].ChangeValue(hud, HudPlayer, "EntWatch Zombies:", "color-red", iSize, "", "", "", "", "", "", 0);

                        for (int i = iCurrentNumListZM * iCountForList; i < ListShowZM.Count && i < (iCurrentNumListZM + 1) * iCountForList; i++)
                        {
                            string sAbilityMessage = "";
                            (string, byte) ColorAbilityProgress = ("color-white", 0);
                            if (!Cvar.TeamOnly || HudPlayer.TeamNum < 2 || ListShowZM[i].Team == HudPlayer.TeamNum || bAdminPermissions && Cvar.AdminHud == 0)
                            {
                                if (ListShowZM[i].CheckDelay())
                                {
                                    int iAbilityCount = 0;
                                    foreach (Ability AbilityTest in ListShowZM[i].AbilityList.ToList())
                                    {
                                        if (iAbilityCount > Cvar.DisplayAbility) break;
                                        if (!AbilityTest.Ignore)
                                        {
                                            iAbilityCount++;
                                            sAbilityMessage += $"|{AbilityTest.GetMessage()}";
                                            if (iAbilityCount == 1) ColorAbilityProgress = AbilityTest.GetColorAndProgress();
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(sAbilityMessage)) sAbilityMessage = sAbilityMessage[1..];
                                    if (iAbilityCount != 1) ColorAbilityProgress = ("color-white", 0);
                                }
                                else
                                {
                                    sAbilityMessage = $"-{Math.Round(ListShowZM[i].fDelay - EW.fGameTime, 1)}";
                                    ColorAbilityProgress = ("color-blue", 0);
                                }
                            }
                            string sOwnerSteam = "";
                            if (ListShowZM[i].Owner is { } client) sOwnerSteam = $"#{client.UserId}|#{EW.ConvertSteamID64ToSteamID(client.SteamID.ToString())}";

                            UHudArray[iCurrentNumHUD].SetButtonItem(hud, HudPlayer);
                            UHudArray[iCurrentNumHUD++].ChangeValue(hud, HudPlayer, ListShowZM[i].ShortName, GetCSSClassColor(ListShowZM[i].Color), iSize, sAbilityMessage, ColorAbilityProgress.Item1, $"{ListShowZM[i].Owner?.PlayerName}", "color-white", sOwnerSteam, "color-grey", ColorAbilityProgress.Item2);
                        }
                        if (iCountListZM > 1)
                        {
                            UHudArray[iCurrentNumHUD].SetButtonItem(hud, HudPlayer);
                            UHudArray[iCurrentNumHUD++].ChangeValue(hud, HudPlayer, "List:", "color-silver", iSize, $"[{iCurrentNumListZM + 1}/{iCountListZM}]", "color-silver", "", "", "", "", 0);
                        }
                    }
                    for (int i = iCurrentNumHUD; i < UHudArray.Length; i++) UHudArray[i].SetButtonNull(hud, HudPlayer);
                }
            }
        }

        public static string GetCSSClassColor(string color)
        {
            if (CSS_Class_Colors.TryGetValue(color.ToLower(), out string classcolor)) return classcolor;
            else return "color-white";
        }

        static readonly Dictionary<string, string> CSS_Class_Colors = new()
        {
            { "{default}", "color-white" },
            { "{darkred}", "color-darkred" },
            { "{purple}", "color-purple" },
            { "{green}", "color-green" },
            { "{lightgreen}", "color-lightgreen" },
            { "{lime}", "color-lime" },
            { "{red}", "color-red" },
            { "{grey}", "color-grey" },
            { "{team}", "color-team" },
            { "{red2}", "color-lightred" },
            { "{olive}", "color-lime" },
            { "{a}", "color-silver" },
            { "{lightblue}", "color-lightblue" },
            { "{blue}", "color-blue" },
            { "{d}", "color-bluegrey" },
            { "{pink}", "color-magenta" },
            { "{darkorange}", "color-lightred" },
            { "{orange}", "color-orange" },
            { "{darkblue}", "color-blue" },
            { "{gold}", "color-orange" },
            { "{white}", "color-white" },
            { "{yellow}", "color-yellow" },
            { "{magenta}", "color-magenta" },
            { "{silver}", "color-silver" },
            { "{bluegrey}", "color-bluegrey" },
            { "{lightred}", "color-lightred" },
            { "{cyan}", "color-team" },
            { "{gray}", "color-grey" },
            { "{lightyellow}", "color-lime" }
        };
    }
}
