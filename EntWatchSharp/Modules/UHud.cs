using CounterStrikeSharp.API.Core;
using EntWatchSharp.Items;
using CounterStrikeSharp.API.Modules.Admin;

namespace EntWatchSharp.Modules
{
    abstract class UHud
    {
        public float fXEntity = -6.5f;
		public float fYEntity = 2.0f;
		public float fZEntity = 7.0f;
		public int[] colorEntity = [255, 255, 255, 255];
		public int iSheetMax = 5;
        public int iRefresh = 3;
        public int iSize = 54;
        int iCurrentNumListH = 0;
        int iCurrentNumListZM = 0;
        double fNextUpdateList = EW.fGameTime - 3;
        bool bNextUpdateSync = true;
        public UHud() { }
        public void ConstructString(CCSPlayerController HudPlayer)
        {
            bNextUpdateSync = true;
            List<Item> ListShowH = [];
            List<Item> ListShowZM = [];
            bool bAdminPermissions = AdminManager.PlayerHasPermissions(HudPlayer, "@css/ew_hud") && Cvar.AdminHud < 2;
			foreach (Item ItemTest in EW.g_ItemList.ToList())
			{
				if (ItemTest.Owner != null)
				{
                    if (ItemTest.Hud && (!Cvar.TeamOnly || HudPlayer.TeamNum < 2 || ItemTest.Team == HudPlayer.TeamNum || bAdminPermissions && Cvar.AdminHud == 0))
					{
                        if (ItemTest.Team == 3) ListShowH.Add(ItemTest);
                        else if (ItemTest.Team == 2) ListShowZM.Add(ItemTest);
                    }
                }
			}
            if (ListShowH.Count > 0 || ListShowZM.Count > 0)
            {
                string sItems = "";
                if (ListShowH.Count > 0)
                {
                    int iCountListH = (ListShowH.Count - 1) / iSheetMax + 1;

                    if (fNextUpdateList <= EW.fGameTime)
                    {
                        iCurrentNumListH++;
                        if (bNextUpdateSync)
                        {
                            fNextUpdateList = EW.fGameTime + iRefresh;
                            bNextUpdateSync = false;
                        }
                    }
                    if (iCurrentNumListH >= iCountListH) iCurrentNumListH = 0;

                    sItems += "EntWatch Humans:";

                    for (int i = iCurrentNumListH * iSheetMax; i < ListShowH.Count && i < (iCurrentNumListH + 1) * iSheetMax; i++)
                    {
                        sItems += $"\n{ListShowH[i].ShortName}";
                        if (ListShowH[i].CheckDelay())
                        {
                            int iAbilityCount = 0;
                            foreach (Ability AbilityTest in ListShowH[i].AbilityList.ToList())
                            {
                                if (++iAbilityCount > Cvar.DisplayAbility) break;
                                if (!AbilityTest.Ignore) sItems += $"[{AbilityTest.GetMessage()}]";
                            }

                        }
                        else sItems += $"[-{Math.Round(ListShowH[i].fDelay - EW.fGameTime, 1)}]";
                        sItems += $": {ListShowH[i].Owner.PlayerName}";
                    }
                    if (iCountListH > 1) sItems += $"\nList:[{iCurrentNumListH + 1}/{iCountListH}]";
                }

                if (ListShowZM.Count > 0)
                {
                    int iCountListZM = (ListShowZM.Count - 1) / iSheetMax + 1;

                    if (bNextUpdateSync == false || fNextUpdateList <= EW.fGameTime)
                    {
                        iCurrentNumListZM++;
                        if (bNextUpdateSync)
                        {
                            fNextUpdateList = EW.fGameTime + iRefresh;
                            bNextUpdateSync = false;
                        }
                    }
                    if (iCurrentNumListZM >= iCountListZM) iCurrentNumListZM = 0;

                    if (!string.IsNullOrEmpty(sItems)) sItems += "\n\n";

                    sItems += "EntWatch Zombies:";

                    for (int i = iCurrentNumListZM * iSheetMax; i < ListShowZM.Count && i < (iCurrentNumListZM + 1) * iSheetMax; i++)
                    {
                        sItems += $"\n{ListShowZM[i].ShortName}";
                        if (ListShowZM[i].CheckDelay())
                        {
                            int iAbilityCount = 0;
                            foreach (Ability AbilityTest in ListShowZM[i].AbilityList.ToList())
                            {
                                if (++iAbilityCount > Cvar.DisplayAbility) break;
                                if (!AbilityTest.Ignore) sItems += $"[{AbilityTest.GetMessage()}]";
                            }

                        }
                        else sItems += $"[-{Math.Round(ListShowZM[i].fDelay - EW.fGameTime, 1)}]";
                        sItems += $": {ListShowZM[i].Owner.PlayerName}";
                    }
                    if (iCountListZM > 1) sItems += $"\nList:[{iCurrentNumListZM + 1}/{iCountListZM}]";
                }
				UpdateText(sItems, HudPlayer);
			}
            else UpdateText("", HudPlayer);
		}
        public abstract void UpdateText(string sItems, CCSPlayerController HudPlayer);
    }

    class HudNull : UHud
    {
        public HudNull() { }
        public override void UpdateText(string sItems, CCSPlayerController HudPlayer) { }
    }

    class HudCenter : UHud
    {
        public HudCenter() { }
        public override void UpdateText(string sItems, CCSPlayerController HudPlayer)
        {
            if (HudPlayer is { IsValid: true, IsBot: false } && !string.IsNullOrEmpty(sItems)) HudPlayer.PrintToCenter(sItems);
        }
    }
    class HudAlert : UHud
    {
        public HudAlert() { }
        public override void UpdateText(string sItems, CCSPlayerController HudPlayer)
        {
            if (HudPlayer is { IsValid: true, IsBot: false } && !string.IsNullOrEmpty(sItems)) HudPlayer.PrintToCenterAlert(sItems);
        }
    }

    class HudWorldText : UHud
    {
		public HudWorldText() { }
		public void InitHud(CCSPlayerController HudPlayer)
		{
			if (EW._GH_api != null && HudPlayer.IsValid)
			{
				EW._GH_api.Native_GameHUD_SetParams(HudPlayer, EW.HUDCHANNEL, fXEntity, fYEntity, fZEntity, System.Drawing.Color.FromArgb(colorEntity[3], colorEntity[0], colorEntity[1], colorEntity[2]), iSize, "Verdana", iSize/7000.0f);
			}
		}
		public override void UpdateText(string sItems, CCSPlayerController HudPlayer)
        {
			if (EW._GH_api != null && HudPlayer.IsValid)
			{
				EW._GH_api.Native_GameHUD_ShowPermanent(HudPlayer, EW.HUDCHANNEL, sItems);
			}
		}
	}
}
