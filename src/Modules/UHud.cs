using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using EntWatchSharp.Items;
using CounterStrikeSharp.API.Modules.Admin;

namespace EntWatchSharp.Modules
{
    abstract class UHud
    {
        public CCSPlayerController HudPlayer;

        public Vector vecEntity = new Vector(50, 50, 50);
        public int iSheetMax = 5;
        public int iRefresh = 3;
        int iCurrentNumList = 0;
        double fNextUpdateList = EW.fGameTime - 3;
		public UHud() { }
        public void ConstructString()
        {
			List<Item> ListShow = new List<Item>();
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
				UpdateText(sItems);
			}
            else UpdateText("");
		}
        public abstract void UpdateText(string sItems);
    }

    class HudNull : UHud
    {
        public HudNull(CCSPlayerController player) { HudPlayer = player; }
        public override void UpdateText(string sItems) { }
    }

    class HudCenter : UHud
    {
        public HudCenter(CCSPlayerController player) { HudPlayer = player; }
        public override void UpdateText(string sItems)
        {
            if (HudPlayer is { IsValid: true, IsBot: false } && !string.IsNullOrEmpty(sItems)) HudPlayer.PrintToCenter(sItems);
        }
    }
    class HudAlert : UHud
    {
        public HudAlert(CCSPlayerController player) { HudPlayer = player; }
        public override void UpdateText(string sItems)
        {
            if (HudPlayer is { IsValid: true, IsBot: false } && !string.IsNullOrEmpty(sItems)) HudPlayer.PrintToCenterAlert(sItems);
        }
    }

    class HudWorldText : UHud
    {
        public CPointWorldText Entity = null;
        public HudWorldText(CCSPlayerController player) { HudPlayer = player; }
        public void CreateHud()
        {
            if (HudPlayer.IsValid && EW.IsPlayerAlive(HudPlayer))
            {
                var entity = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext");
                if (entity == null || !entity.IsValid) return;

                QAngle vAngle = new QAngle(HudPlayer.PlayerPawn.Value?.AbsRotation?.X, HudPlayer.PlayerPawn.Value?.AbsRotation?.Y, HudPlayer.PlayerPawn.Value?.AbsRotation?.Z);

                HudPlayer.Pawn.Value?.Teleport(HudPlayer.PlayerPawn.Value?.AbsOrigin, new QAngle(0, 0, 0), HudPlayer.PlayerPawn.Value?.AbsVelocity);

                entity.Teleport(
                    new Vector(
                HudPlayer.PlayerPawn.Value?.AbsOrigin?.X + vecEntity.X,
                HudPlayer.PlayerPawn.Value?.AbsOrigin?.Y + vecEntity.Y,
                HudPlayer.PlayerPawn.Value?.AbsOrigin?.Z + vecEntity.Z
                ),
                new QAngle(0, 270, 75),
                HudPlayer.PlayerPawn.Value?.AbsVelocity);
                entity.FontSize = 18;
                entity.FontName = "Consolas";
                entity.Enabled = true;
                entity.Fullbright = true;
                entity.WorldUnitsPerPx = 0.25f;
                entity.Color = System.Drawing.Color.White;
                entity.MessageText = "";
                entity.DispatchSpawn();
                entity.AcceptInput("SetParent", HudPlayer.PlayerPawn.Value, null, "!activator");
                Entity = entity;

                HudPlayer.Pawn.Value?.Teleport(HudPlayer.PlayerPawn.Value?.AbsOrigin, vAngle, HudPlayer.PlayerPawn.Value?.AbsVelocity);
            }
        }
        public override void UpdateText(string sItems)
        {
            if (Entity != null && Entity.IsValid)
            {
                if (!string.IsNullOrEmpty(sItems)) Entity.AcceptInput("SetMessage", null, null, sItems);
                else Entity.AcceptInput("SetMessage", null, null, "");
            }
        }
    }
}
