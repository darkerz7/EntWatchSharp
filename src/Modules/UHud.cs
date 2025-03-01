using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using EntWatchSharp.Items;
using CounterStrikeSharp.API.Modules.Admin;

namespace EntWatchSharp.Modules
{
    abstract class UHud
    {
        public Vector vecEntity = new(-8, 2, 7);
        public int[] colorEntity = [255, 255, 255, 255];
		public int iSheetMax = 5;
        public int iRefresh = 3;
        public int iSize = 54;
        int iCurrentNumList = 0;
        double fNextUpdateList = EW.fGameTime - 3;
		public UHud() { }
        public void ConstructString(CCSPlayerController HudPlayer)
        {
			List<Item> ListShow = [];
			bool bAdminPermissions = AdminManager.PlayerHasPermissions(HudPlayer, "@css/ew_hud") && Cvar.AdminHud < 2;
			foreach (Item ItemTest in EW.g_ItemList.ToList())
			{
				if (ItemTest.Owner != null)
				{
					if (ItemTest.Hud && (!Cvar.TeamOnly || HudPlayer.TeamNum < 2 || ItemTest.Team == HudPlayer.TeamNum || bAdminPermissions))
					{
						ListShow.Add(ItemTest);
					}
				}
			}
            if (ListShow.Count > 0)
            {
                int iCountList = (ListShow.Count - 1) / iSheetMax + 1;

                if (fNextUpdateList <= EW.fGameTime)
                {
                    iCurrentNumList++;
                    fNextUpdateList = EW.fGameTime + iRefresh;
                }
                if (iCurrentNumList >= iCountList) iCurrentNumList = 0;

				string sItems = "EntWatch:";

                for(int i = iCurrentNumList * iSheetMax; i < ListShow.Count && i < (iCurrentNumList + 1) * iSheetMax; i++)
                {
					sItems += $"\n{ListShow[i].ShortName}";
					if (!Cvar.TeamOnly || HudPlayer.TeamNum < 2 || ListShow[i].Team == HudPlayer.TeamNum || bAdminPermissions && Cvar.AdminHud == 0)
					{
						if (ListShow[i].CheckDelay())
						{
							int iAbilityCount = 0;
							foreach (Ability AbilityTest in ListShow[i].AbilityList.ToList())
							{
								if (++iAbilityCount > Cvar.DisplayAbility) break;
								if (!AbilityTest.Ignore) sItems += $"[{AbilityTest.GetMessage()}]";
							}

						}
						else sItems += $"[-{Math.Round(ListShow[i].fDelay - EW.fGameTime, 1)}]";
					}
                    sItems += $": {ListShow[i].Owner.PlayerName}";
				}
                if(iCountList > 1) sItems += $"\nList:[{iCurrentNumList+1}/{iCountList}]";
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
				EW._GH_api.Native_GameHUD_SetParams(HudPlayer, EW.HUDCHANNEL, vecEntity, System.Drawing.Color.FromArgb(colorEntity[3], colorEntity[0], colorEntity[1], colorEntity[2]), iSize, "Verdana", iSize/540.0f);
			}
		}
		public override void UpdateText(string sItems, CCSPlayerController HudPlayer)
        {
			if (EW._GH_api != null && HudPlayer.IsValid)
			{
				EW._GH_api.Native_GameHUD_Show(HudPlayer, EW.HUDCHANNEL, sItems, 1.2f);
			}
		}
	}
}
