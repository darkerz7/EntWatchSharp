﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using EntWatchSharp.Items;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Memory;

namespace EntWatchSharp.Modules
{
    abstract class UHud
    {
        public Vector vecEntity = new(-100, 25, 80);
        public int[] colorEntity = [255, 255, 255, 255];
		public int iSheetMax = 5;
        public int iRefresh = 3;
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
        public CPointWorldText Entity = null;
		public HudWorldText() { }
		/*public void CreateHud()
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
        }*/
		public void CreateHud(CCSPlayerController HudPlayer)
		{
			if (HudPlayer.IsValid && EW.IsPlayerAlive(HudPlayer))
			{
				CCSPlayerPawn pawn = HudPlayer.PlayerPawn.Value!;
				var handle = new CHandle<CCSGOViewModel>((IntPtr)(pawn.ViewModelServices!.Handle + Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel") + 4));
				if (!handle.IsValid)
				{
					CCSGOViewModel viewmodel = Utilities.CreateEntityByName<CCSGOViewModel>("predicted_viewmodel")!;
					viewmodel.DispatchSpawn();
					handle.Raw = viewmodel.EntityHandle.Raw;
					Utilities.SetStateChanged(pawn, "CCSPlayerPawnBase", "m_pViewModelServices");
				}
				CPointWorldText entity = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext")!;
				entity.FontSize = 18;
				//entity.FontName = "Consolas";
				entity.FontName = "Verdana";
				entity.Enabled = true;
				entity.Fullbright = true;
				entity.WorldUnitsPerPx = 0.25f;
                entity.Color = System.Drawing.Color.FromArgb(colorEntity[3], colorEntity[0], colorEntity[1], colorEntity[2]); //System.Drawing.Color.White;
				entity.MessageText = "";
				entity.JustifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT;
				entity.JustifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP;
				entity.ReorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;

				QAngle eyeAngles = pawn.EyeAngles;
				Vector forward = new(), right = new(), up = new();
				NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, right.Handle, up.Handle);

				Vector eyePosition = new();
				eyePosition += forward * vecEntity.Z;
				eyePosition += right * vecEntity.X;
				eyePosition += up * vecEntity.Y;
				QAngle angles = new()
				{
					Y = eyeAngles.Y + 270,
					Z = 90 - eyeAngles.X,
					X = 0
				};

				entity.DispatchSpawn();
				entity.Teleport(pawn.AbsOrigin! + eyePosition + new Vector(0, 0, pawn.ViewOffset.Z), angles, null);
				entity.AcceptInput("SetParent", handle.Value, null, "!activator");

				Entity = entity;
			}
		}
		public override void UpdateText(string sItems, CCSPlayerController HudPlayer)
        {
            if (Entity != null && Entity.IsValid)
            {
                if (!string.IsNullOrEmpty(sItems)) Entity.AcceptInput("SetMessage", null, null, sItems);
                else Entity.AcceptInput("SetMessage", null, null, "");
            }
        }
	}
}
