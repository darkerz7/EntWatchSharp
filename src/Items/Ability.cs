using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using System;

namespace EntWatchSharp.Items
{
    public class Ability
    {
        public string Name { get; set; }
        public string ButtonClass { get; set; } //func_button, func_door, game_ui, func_physbox
        public bool Chat_Uses { get; set; }
        public int Mode { get; set; }
        public int MaxUses { get; set; }
        public int CoolDown { get; set; }
        public int ButtonID { get; set; }
        public bool Ignore { get; set; }
        public bool LockItem { get; set; }
		public int MathID { get; set; }
        public bool MathNameFix { get; set; }
		public string Filter { get; set; } // <activatorname> or <Context:1> or <$attribute>

		public CEntityInstance Entity;
        public CMathCounter MathCounter;
        public double fLastUse;
        public int iCurrentUses;

        public Ability()
        {
            Name = "";
            ButtonClass = "";
            Chat_Uses = true;
            Mode = 0;
            MaxUses = 0;
            CoolDown = 0;
            ButtonID = 0;
            Ignore = false;
            LockItem = false;
			MathID = 0;
			MathNameFix = false;
            Filter = "";

			Entity = null;
			MathCounter = null;
			fLastUse = 0.0;
            iCurrentUses = 0;
        }
        public Ability(string name, string buttonclass, bool chat_uses, int mode, int maxuses, int cooldown, int buttonid, CEntityInstance entity = null)
        {
            Name = name;
            ButtonClass = buttonclass;
            Chat_Uses = chat_uses;
            Mode = mode;
            MaxUses = maxuses;
            CoolDown = cooldown;
            ButtonID = buttonid;
            Ignore = false;
            LockItem = false;
			MathID = 0;
			MathNameFix = false;
			Filter = "";

			Entity = entity;
            MathCounter = null;

			EW.UpdateTime();
            fLastUse = EW.fGameTime - CoolDown;
            iCurrentUses = 0;
        }

		public Ability(Ability cCopyAbility)
		{
			Name = cCopyAbility.Name;
			ButtonClass = cCopyAbility.ButtonClass;
			Chat_Uses = cCopyAbility.Chat_Uses;
			Mode = cCopyAbility.Mode;
			MaxUses = cCopyAbility.MaxUses;
			CoolDown = cCopyAbility.CoolDown;
			ButtonID = cCopyAbility.ButtonID;
            Ignore = cCopyAbility.Ignore;
            LockItem = cCopyAbility.LockItem;
            MathID = cCopyAbility.MathID;
            MathNameFix = cCopyAbility.MathNameFix;
            Filter = cCopyAbility.Filter;

			Entity = null;
			MathCounter = null;
			EW.UpdateTime();
			fLastUse = EW.fGameTime - CoolDown;
			iCurrentUses = 0;
		}

        public void SetFilter(CEntityInstance activator)
        {
			if (!string.IsNullOrEmpty(Filter))
			{
                if (activator.DesignerName.CompareTo("player") != 0) return;
                CCSPlayerPawn pawn = new CCSPlayerController(activator.Handle).PlayerPawn.Value;
                if (pawn == null || !pawn.IsValid) return;

				if (Filter[0] == '$')
                {
                    if (Filter.Length > 1) pawn.AcceptInput("AddAttribute", null, null, Filter.Substring(1));
                }
                else if (Filter.Contains(":"))
                {
					pawn.AcceptInput("AddContext", null, null, Filter);
                }
                else
                {
					if (pawn.Entity != null) pawn.Entity.Name = Filter;
                }
			}
		}
#nullable enable
		private CCSPlayerController? EntityIsPlayer(CEntityInstance? entity)
#nullable disable
		{
			if (entity != null && entity.IsValid && entity.DesignerName.CompareTo("player") == 0)
			{
				var pawn = new CCSPlayerPawn(entity.Handle);
				if (pawn.Controller.Value != null && pawn.Controller.Value.IsValid)
				{
					var player = new CCSPlayerController(pawn.Controller.Value.Handle);
					if (player != null && player.IsValid) return player;
				}
			}
			return null;
		}

		public void Used()
        {
            switch (Mode)
            {
                case 2:
                    if (fLastUse < EW.fGameTime) fLastUse = EW.fGameTime + CoolDown;
                    break;
                case 3:
                    if (iCurrentUses < MaxUses)
                    {
                        iCurrentUses++;
                        fLastUse = EW.fGameTime + 1;
					}
                    break;
                case 4:
                    if (iCurrentUses < MaxUses)
                    {
                        iCurrentUses++;
                        fLastUse = EW.fGameTime + CoolDown;
                    }
                    break;
                case 5:
                    iCurrentUses++;
					fLastUse = EW.fGameTime + 1;
					if (iCurrentUses == MaxUses)
                    {
                        fLastUse = EW.fGameTime + CoolDown;
                        iCurrentUses = 0;
                    }
                    break;
            }
        }

        public string GetMessage()
        {
            switch (Mode)
            {
                case 2:
                    if (fLastUse < EW.fGameTime) return "R";
                    else return $"{Math.Round(fLastUse - EW.fGameTime, 0)}";
                case 3:
                    if (iCurrentUses < MaxUses) return $"{iCurrentUses}/{MaxUses}";
                    else return "E";
                case 4:
                    if (fLastUse < EW.fGameTime)
                    {
                        if (iCurrentUses < MaxUses) return $"{iCurrentUses}/{MaxUses}";
                        else return "E";
                    }
                    else return $"{Math.Round(fLastUse - EW.fGameTime, 0)}";
                case 5:
                    if (fLastUse < EW.fGameTime) return $"{iCurrentUses}/{MaxUses}";
                    else return $"{Math.Round(fLastUse - EW.fGameTime, 0)}";
                case 6:
                    {
                        if (MathCounter != null && MathCounter.IsValid)
                        {
                            float fValue = EntWatchSharp.MathCounter_GetValue(MathCounter);
                            if (fValue > MathCounter.Min) return $"{fValue.ToString("F1")}/{MathCounter.Max.ToString("F1")}";
                            else return "E";
                        }
                        else return "+";
                    }
                case 7:
                    {
                        if (MathCounter != null && MathCounter.IsValid) 
                        {
                            float fValue = MathCounter.Max - EntWatchSharp.MathCounter_GetValue(MathCounter);
							if(fValue < MathCounter.Max) return $"{fValue.ToString("F1")}/{MathCounter.Max.ToString("F1")}";
                            else return "E";
						}
                        else return "+";
                    }
                case 8:
                    {
                        if (Entity != null && Entity.IsValid)
                        {
                            CBaseEntity cBaseEntity = new CBaseEntity(Entity.Handle);
                            return $"{cBaseEntity.Health} HP";
                        }
						else return "+";
					}

                default: return "+";
            }
        }
		public bool Ready()
		{
            // Maybe not needed...
            /*if (Entity.DesignerName.CompareTo("func_button") == 0 || Entity.DesignerName.CompareTo("func_rot_button") == 0)
            {
                if (new CBaseButton(Entity.Handle).Locked) return false;
            }
            else if (Entity.DesignerName.CompareTo("func_door") == 0 || Entity.DesignerName.CompareTo("func_door_rotating") == 0)
            {
                if (new CBaseDoor(Entity.Handle).Locked) return false;
            }
            else if (Entity.DesignerName.CompareTo("func_physbox") == 0)
            {
				if (!new CPhysBox(Entity.Handle).EnableUseOutput) return false;
			}
            else return false;*/
            if (LockItem) return false;
            if (fLastUse >= EW.fGameTime) return false;
			switch (Mode)
			{
				case 2: return true;
				case 3:
                    if (iCurrentUses < MaxUses) return true;
                    else return false;
				case 4:
					if (iCurrentUses < MaxUses) return true;
					else return false;
				case 5: return true;
                case 6:
                     if (MathCounter != null && MathCounter.IsValid && EntWatchSharp.MathCounter_GetValue(MathCounter) > MathCounter.Min) return true;
                     else return false;
                case 7:
					if (MathCounter != null && MathCounter.IsValid && (MathCounter.Max - EntWatchSharp.MathCounter_GetValue(MathCounter)) < MathCounter.Max) return true;
					else return false;
                case 8: return false;
				default: return true;
			}
		}
	}
}
