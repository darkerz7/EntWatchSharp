using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Extensions;
using EntWatchSharp.Items;

namespace EntWatchSharp.Modules
{
    class UHudItem
    {
        bool bNull = false;
        string sItemText = "";
        string sItemColorClass = "";
        byte iTextSize = 99; //small - 0, normal - 1, big - 2, large - 3
        string sProgressColorClass = "";
        byte iProgressValue = 0;

        readonly string __Cache_Item;
        readonly string __Cache_ProgressBar;
        readonly string __Cache_Button;
        readonly string __Cache_Item_Text;

        public UHudItem(byte iNum)
        {
            string sNum = iNum.ToString("D2");
            __Cache_Item = $"entwatch-item-{sNum}";
            __Cache_ProgressBar = $"entwatch-pb-{sNum}";
            __Cache_Button = $"entwatch-btn-{sNum}";
            __Cache_Item_Text = $"entwatch-item-text-{sNum}";
        }

        public void SetButtonNull(CCSCustomHudLayout eHud, CCSPlayerController HudPlayer)
        {
            if (!bNull)
            {
                eHud.SetHasClassForPlayer(HudPlayer, __Cache_Button, "null", true);
                ChangeValue(eHud, HudPlayer, "", sItemColorClass, iTextSize, 0, "");
                bNull = true;
            }
        }

        public void SetButtonItem(CCSCustomHudLayout eHud, CCSPlayerController HudPlayer)
        {
            if (bNull)
            {
                eHud.SetHasClassForPlayer(HudPlayer, __Cache_Button, "null", false);
                bNull = false;
            }
        }

        public void ChangeValue(CCSCustomHudLayout eHud, CCSPlayerController HudPlayer, string _ItemText, string _ItemColorClass, byte _TextSize, byte _ProgressValue, string _ProgressColorClass)
        {
            if (!string.Equals(sItemText, _ItemText, StringComparison.Ordinal))
            {
                eHud.SetDialogVariableStringForPlayer(HudPlayer, __Cache_Item, __Cache_Item_Text, _ItemText);
                sItemText = _ItemText;
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

            if (UpdateColorClasses(sItemColorClass, _ItemColorClass, __Cache_Item)) sItemColorClass = _ItemColorClass;
            if (UpdateColorClasses($"bg-{sProgressColorClass}", $"bg-{_ProgressColorClass}", __Cache_ProgressBar)) sProgressColorClass = _ProgressColorClass;

            if (iTextSize != _TextSize)
            {
                void UpdateSizeClasses(int iTextSizeValue, bool status)
                {
                    string sSizeAll = iTextSizeValue switch
                    {
                        0 => "size-small",
                        1 => "size-normal",
                        2 => "size-big",
                        3 => "size-large",
                        _ => "size-normal"
                    };

                    eHud.SetHasClassForPlayer(HudPlayer, __Cache_Item, sSizeAll, status);
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
        bool bSeparator = false;
        readonly UHudItem[] UHudArray = new UHudItem[14];
        public int iRefresh = 3;
        public byte iSize = 1;
        public byte iPosition = 0;
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

        public void ChangePosition(CCSPlayerController HudPlayer, byte _Position)
        {
            if (iPosition != _Position)
            {
                if (EW.GetorCreateHudLayout() is { } hud)
                {
                    hud.SetHasClassForPlayer(HudPlayer, "ew-panel", $"panel-valign-{iPosition}", false);
                    hud.SetHasClassForPlayer(HudPlayer, "ew-panel", $"panel-valign-{_Position}", true);
                }

                iPosition = _Position;
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
                        if (ListShowZM.Count == 0)
                        {
                            iCountForList = 10;
                            SetSeparator(hud, HudPlayer, false);
                        }
                        int iCountListH = (ListShowH.Count - 1) / iCountForList + 1;

                        if (fNextUpdateList <= EW.fGameTime)
                        {
                            iCurrentNumListH++;
                            fNextUpdateList = EW.fGameTime + iRefresh;
                            bNextUpdateSync = false;
                        }
                        if (iCurrentNumListH >= iCountListH) iCurrentNumListH = 0;

                        UHudArray[iCurrentNumHUD].SetButtonItem(hud, HudPlayer);
                        UHudArray[iCurrentNumHUD++].ChangeValue(hud, HudPlayer, "EntWatch Humans:", "color-lightblue", iSize, 0, "");

                        for (int i = iCurrentNumListH * iCountForList; i < ListShowH.Count && i < (iCurrentNumListH + 1) * iCountForList; i++)
                        {
                            string sMessage = ListShowH[i].ShortName;
                            (string, byte) ColorProgress = ("color-white", 0);
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
                                            sMessage += $"|{AbilityTest.GetMessage()}";
                                            if (iAbilityCount == 1) ColorProgress = AbilityTest.GetColorAndProgress();
                                        }
                                    }
                                    if (iAbilityCount != 1) ColorProgress = ("color-white", 0);
                                }
                                else
                                {
                                    sMessage += $"|-{Math.Round(ListShowH[i].fDelay - EW.fGameTime, 1)}";
                                    ColorProgress = ("color-blue", 0);
                                }
                            }
                            sMessage += $": {ListShowH[i].Owner?.PlayerName}";

                            UHudArray[iCurrentNumHUD].SetButtonItem(hud, HudPlayer);
                            UHudArray[iCurrentNumHUD++].ChangeValue(hud, HudPlayer, sMessage, ColorProgress.Item1, iSize, ColorProgress.Item2, GetCSSClassColor(ListShowH[i].Color));
                        }
                        if (iCountListH > 1)
                        {
                            UHudArray[iCurrentNumHUD].SetButtonItem(hud, HudPlayer);
                            UHudArray[iCurrentNumHUD++].ChangeValue(hud, HudPlayer, $"List: [{iCurrentNumListH + 1}/{iCountListH}]", "color-silver", iSize, 0, "");
                        }
                        for (; iCurrentNumHUD < 7; iCurrentNumHUD++) UHudArray[iCurrentNumHUD].SetButtonNull(hud, HudPlayer);
                    }

                    if (ListShowZM.Count > 0)
                    {
                        byte iCountForList = 5;
                        if (ListShowH.Count == 0)
                        {
                            iCountForList = 10;
                            SetSeparator(hud, HudPlayer, false);
                        }
                        else SetSeparator(hud, HudPlayer, true);

                        int iCountListZM = (ListShowZM.Count - 1) / iCountForList + 1;

                        if (!bNextUpdateSync || fNextUpdateList <= EW.fGameTime)
                        {
                            iCurrentNumListZM++;
                            if (bNextUpdateSync) fNextUpdateList = EW.fGameTime + iRefresh;
                        }
                        if (iCurrentNumListZM >= iCountListZM) iCurrentNumListZM = 0;

                        UHudArray[iCurrentNumHUD].SetButtonItem(hud, HudPlayer);
                        UHudArray[iCurrentNumHUD++].ChangeValue(hud, HudPlayer, "EntWatch Zombies:", "color-red", iSize, 0, "");

                        for (int i = iCurrentNumListZM * iCountForList; i < ListShowZM.Count && i < (iCurrentNumListZM + 1) * iCountForList; i++)
                        {
                            string sMessage = ListShowZM[i].ShortName;
                            (string, byte) ColorProgress = ("color-white", 0);
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
                                            sMessage += $"|{AbilityTest.GetMessage()}";
                                            if (iAbilityCount == 1) ColorProgress = AbilityTest.GetColorAndProgress();
                                        }
                                    }
                                    if (iAbilityCount != 1) ColorProgress = ("color-white", 0);
                                }
                                else
                                {
                                    sMessage += $"-{Math.Round(ListShowZM[i].fDelay - EW.fGameTime, 1)}";
                                    ColorProgress = ("color-blue", 0);
                                }
                            }
                            sMessage += $": {ListShowZM[i].Owner?.PlayerName}";

                            UHudArray[iCurrentNumHUD].SetButtonItem(hud, HudPlayer);
                            UHudArray[iCurrentNumHUD++].ChangeValue(hud, HudPlayer, sMessage, ColorProgress.Item1, iSize, ColorProgress.Item2, GetCSSClassColor(ListShowZM[i].Color));
                        }
                        if (iCountListZM > 1)
                        {
                            UHudArray[iCurrentNumHUD].SetButtonItem(hud, HudPlayer);
                            UHudArray[iCurrentNumHUD++].ChangeValue(hud, HudPlayer, $"List: [{iCurrentNumListZM + 1}/{iCountListZM}]", "color-silver", iSize, 0, "");
                        }
                    }
                    for (; iCurrentNumHUD < UHudArray.Length; iCurrentNumHUD++) UHudArray[iCurrentNumHUD].SetButtonNull(hud, HudPlayer);
                }
            }
        }

        void SetSeparator(CCSCustomHudLayout eHud, CCSPlayerController HudPlayer, bool bEnabled)
        {
            if (bSeparator == bEnabled) return;

            eHud.SetHasClassForPlayer(HudPlayer, "entwatch-separator", "show", bEnabled);
            bSeparator = bEnabled;
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
