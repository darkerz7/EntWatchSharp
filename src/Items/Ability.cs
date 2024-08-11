using CounterStrikeSharp.API.Core;

namespace EntWatchSharp.Items
{
    internal class Ability
    {
        public string Name { get; set; }
        public string ButtonClass { get; set; } //func_button, func_door, game_ui
        public bool Chat_Uses { get; set; }
        public int Mode { get; set; }
        public int MaxUses { get; set; }
        public int CoolDown { get; set; }
        public int ButtonID { get; set; }

        public CEntityInstance Entity;
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

            Entity = null;
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

            Entity = entity;
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

			Entity = null;
			EW.UpdateTime();
			fLastUse = EW.fGameTime - CoolDown;
			iCurrentUses = 0;
		}

		public void Used()
        {
            switch (Mode)
            {
                case 2:
                    if (fLastUse < EW.fGameTime) fLastUse = EW.fGameTime + CoolDown;
                    break;
                case 3:
                    if (iCurrentUses < MaxUses) iCurrentUses++;
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
                default: return "+";
            }
        }
		public bool Ready()
		{
			switch (Mode)
			{
				case 2:
					if (fLastUse < EW.fGameTime) return true;
					else return false;
				case 3:
                    if (iCurrentUses < MaxUses) return true;
                    else return false;
				case 4:
					if (fLastUse < EW.fGameTime)
					{
						if (iCurrentUses < MaxUses) return true;
						else return false;
					}
					else return false;
				case 5:
					if (fLastUse < EW.fGameTime) return true;
					else return false;
				default: return true;
			}
		}
	}
}
