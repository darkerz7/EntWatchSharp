using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using EntWatchSharp.Helpers;
using EntWatchSharp.Items;

namespace EntWatchSharp
{
	public delegate void MapCommAbilityFunc<T>(Ability AbilityTest, T Value);
	public delegate void MapCommItemFunc<T>(Item ItemTest, T Value);
	public static class MapCommands<T>
	{
		public static void ForAllAbilities(CommandInfo command, MapCommAbilityFunc<T> mc_func, T Value)
		{
			if(!Int32.TryParse(command.GetArg(1), out int iHammerID) || iHammerID < 0 || !Int32.TryParse(command.GetArg(2), out int iButtonID) || iButtonID < 0) return;

			EW.UpdateTime();

			foreach (Item ItemTest in EW.g_ItemList.ToList())
			{
				if (ItemTest.HammerID == iHammerID)
				{
					foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
					{
						if (iButtonID == AbilityTest.ButtonID || iButtonID == 0) mc_func(AbilityTest, Value);
					}
				}
			}
		}
		
		public static void ForAllItems(CommandInfo command, MapCommItemFunc<T> mc_func, T Value)
		{
			if(!Int32.TryParse(command.GetArg(1), out int iHammerID) || iHammerID < 0) return;

			foreach (Item ItemTest in EW.g_ItemList.ToList())
			{
				if (ItemTest.HammerID == iHammerID)
				{
					mc_func(ItemTest, Value);
				}
			}
		}

		public static MapCommAbilityFunc<MCS_ValueIntBool> DG_SetCoolDown = (Ability AbilityTest, MCS_ValueIntBool Value) =>
		{
			AbilityTest.CoolDown = Value.iVal;
			if (Value.bFlag) AbilityTest.fLastUse = EW.fGameTime + AbilityTest.CoolDown;
			if (Cvar.DisplayMapCommands) UI.EWSysInfo("Info.MapCommands.SetCooldown", 4, AbilityTest.ButtonID, Value.iVal, Value.bFlag);
		};

		public static MapCommAbilityFunc<MCS_ValueIntBool> DG_SetMaxUses = (Ability AbilityTest, MCS_ValueIntBool Value) =>
		{
			if (AbilityTest.MaxUses > AbilityTest.iCurrentUses || Value.bFlag)
			{
				AbilityTest.MaxUses = Value.iVal;
				if (Cvar.DisplayMapCommands) UI.EWSysInfo("Info.MapCommands.MaxUses", 4, AbilityTest.ButtonID, Value.iVal, Value.bFlag);
			}
		};
		
		public static MapCommAbilityFunc<MCS_ValueIntBool> DG_SetUses = (Ability AbilityTest, MCS_ValueIntBool Value) =>
		{
			if (AbilityTest.MaxUses > AbilityTest.iCurrentUses || Value.bFlag)
			{
				AbilityTest.iCurrentUses = Value.iVal;
				if (Cvar.DisplayMapCommands) UI.EWSysInfo("Info.MapCommands.SetCurrentUses", 4, AbilityTest.ButtonID, Value.iVal, Value.bFlag);
			}
		};
		
		public static MapCommAbilityFunc<bool> DG_AddMaxUses = (Ability AbilityTest, bool Value) =>
		{
			if (AbilityTest.MaxUses > AbilityTest.iCurrentUses || Value)
			{
				AbilityTest.MaxUses++;
				if (Cvar.DisplayMapCommands) UI.EWSysInfo("Info.MapCommands.AddMaxUses", 4, AbilityTest.ButtonID, Value);
			}
		};
		
		public static MapCommAbilityFunc<MCS_ValueMode> DG_SetMode = (Ability AbilityTest, MCS_ValueMode Value) =>
		{
			if (AbilityTest.MaxUses > AbilityTest.iCurrentUses || Value.bFlag || Value.iMode == 7 || Value.iMode == 6 || Value.iMode == 2 || Value.iMode == 1)
			{
				AbilityTest.Mode = Value.iMode;
				AbilityTest.CoolDown = Value.iCooldown;
				AbilityTest.MaxUses = Value.iMaxuses;
				if (Cvar.DisplayMapCommands) UI.EWSysInfo("Info.MapCommands.SetMode", 4, AbilityTest.ButtonID, Value.iMode, Value.iCooldown, Value.iMaxuses, Value.bFlag);
			}
		};
		
		public static MapCommAbilityFunc<bool> DG_LockButton = (Ability AbilityTest, bool Value) =>
		{
			AbilityTest.LockItem = Value;
			if (Cvar.DisplayMapCommands) UI.EWSysInfo("Info.MapCommands.LockButton", 4, AbilityTest.ButtonID, Value);
		};
		
		public static MapCommAbilityFunc<string> DG_SetAbilityName = (Ability AbilityTest, string Value) =>
		{
			AbilityTest.Name = Value;
			if (Cvar.DisplayMapCommands) UI.EWSysInfo("Info.MapCommands.SetAbilityName", 4, AbilityTest.ButtonID, Value);
		};
		
		public static MapCommItemFunc<string> DG_SetFullName = (Item ItemTest, string Value) =>
		{
			ItemTest.Name = Value;
			if (Cvar.DisplayMapCommands) UI.EWSysInfo("Info.MapCommands.SetFullName", 4, ItemTest.HammerID, Value);
		};
		
		public static MapCommItemFunc<string> DG_SetShortName = (Item ItemTest, string Value) =>
		{
			ItemTest.ShortName = Value;
			if (Cvar.DisplayMapCommands) UI.EWSysInfo("Info.MapCommands.SetShortName", 4, ItemTest.HammerID, Value);
		};
		
		public static MapCommItemFunc<bool> DG_Block = (Item ItemTest, bool Value) =>
		{
			ItemTest.BlockPickup = Value;
			if (ItemTest.WeaponHandle != null && ItemTest.WeaponHandle.IsValid) ItemTest.WeaponHandle.CanBePickedUp = !Value;
			if (Cvar.DisplayMapCommands) UI.EWSysInfo("Info.MapCommands.Block", 4, ItemTest.HammerID, Value);
		};
	}

	public struct MCS_ValueIntBool
	{
		public int iVal;
		public bool bFlag;
	}
	
	public struct MCS_ValueMode
	{
		public int iMode;
		public int iCooldown;
		public int iMaxuses;
		public bool bFlag;
	}

	public partial class EntWatchSharp : BasePlugin
	{
		void UnRegMapCommands()
		{
			RemoveCommand("ew_setcooldown", OnEWMC_SetCooldown);
			RemoveCommand("ew_setmaxuses", OnEWMC_SetMaxUses);
			RemoveCommand("ew_setuses", OnEWMC_SetUses);
			RemoveCommand("ew_addmaxuses", OnEWMC_AddMaxUses);
			RemoveCommand("ew_setmode", OnEWMC_SetMode);
			RemoveCommand("ew_lockbutton", OnEWMC_LockButton);
			RemoveCommand("ew_setabilityname", OnEWMC_SetAbilityName);
			RemoveCommand("ew_setname", OnEWMC_SetFullName);
			RemoveCommand("ew_setshortname", OnEWMC_SetShortName);
			RemoveCommand("ew_block", OnEWMC_Block);
		}

		[ConsoleCommand("ew_setcooldown", "Allows you to change the item’s cooldown during the game")]
		[RequiresPermissions("@css/ew_map")]
		[CommandHelper(minArgs: 3, usage: "<hammerid> <buttonid> <new cooldown> [<force apply>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWMC_SetCooldown(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!EW.g_CfgLoaded || command.ArgCount < 4) return;
			MCS_ValueIntBool Value = new MCS_ValueIntBool();
			if (!Int32.TryParse(command.GetArg(3), out Value.iVal) || Value.iVal < 0) return;

			Value.bFlag = false;
			if (command.ArgCount >= 5)
			{
				if (!bool.TryParse(command.GetArg(4), out Value.bFlag))
					if (Int32.TryParse(command.GetArg(3), out int iFBuf) && iFBuf == 1) Value.bFlag = true;
			}

			MapCommands<MCS_ValueIntBool>.ForAllAbilities(command, MapCommands<MCS_ValueIntBool>.DG_SetCoolDown, Value);
		}

		[ConsoleCommand("ew_setmaxuses", "Allows you to change the maximum use of the item during the game, depending on whether the item was used to the end")]
		[RequiresPermissions("@css/ew_map")]
		[CommandHelper(minArgs: 3, usage: "<hammerid> <buttonid> <maxuses> [<even if over>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWMC_SetMaxUses(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!EW.g_CfgLoaded || command.ArgCount < 4) return;
			MCS_ValueIntBool Value = new MCS_ValueIntBool();
			if (!Int32.TryParse(command.GetArg(3), out Value.iVal) || Value.iVal < 0) return;

			Value.bFlag = false;
			if (command.ArgCount >= 5)
			{
				if (!bool.TryParse(command.GetArg(4), out Value.bFlag))
					if (Int32.TryParse(command.GetArg(4), out int iFBuf) && iFBuf == 1) Value.bFlag = true;
			}

			MapCommands<MCS_ValueIntBool>.ForAllAbilities(command, MapCommands<MCS_ValueIntBool>.DG_SetMaxUses, Value);
		}
		
		[ConsoleCommand("ew_setuses", "Allows you to change the current use of the item during the game, depending on whether the item was used to the end")]
		[RequiresPermissions("@css/ew_map")]
		[CommandHelper(minArgs: 3, usage: "<hammerid> <buttonid> <value> [<even if over>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWMC_SetUses(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!EW.g_CfgLoaded || command.ArgCount < 4) return;
			MCS_ValueIntBool Value = new MCS_ValueIntBool();
			if (!Int32.TryParse(command.GetArg(3), out Value.iVal) || Value.iVal < 0) return;

			Value.bFlag = false;
			if (command.ArgCount >= 5)
			{
				if (!bool.TryParse(command.GetArg(4), out Value.bFlag))
					if (Int32.TryParse(command.GetArg(4), out int iFBuf) && iFBuf == 1) Value.bFlag = true;
			}

			MapCommands<MCS_ValueIntBool>.ForAllAbilities(command, MapCommands<MCS_ValueIntBool>.DG_SetUses, Value);
		}
		
		[ConsoleCommand("ew_addmaxuses", "Allows you to add 1 charge to the item, depending on whether the item was used to the end")]
		[RequiresPermissions("@css/ew_map")]
		[CommandHelper(minArgs: 2, usage: "<hammerid> <buttonid> [<even if over>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWMC_AddMaxUses(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!EW.g_CfgLoaded || command.ArgCount < 3) return;

			bool bFlag = false;
			if (command.ArgCount >= 4)
			{
				if (!bool.TryParse(command.GetArg(3), out bFlag))
					if (Int32.TryParse(command.GetArg(3), out int iFBuf) && iFBuf == 1) bFlag = true;
			}

			MapCommands<bool>.ForAllAbilities(command, MapCommands<bool>.DG_AddMaxUses, bFlag);
		}
		
		[ConsoleCommand("ew_setmode", "Allows you to completely change the item")]
		[RequiresPermissions("@css/ew_map")]
		[CommandHelper(minArgs: 5, usage: "<hammerid> <buttonid> <newmode> <cooldown> <maxuses> [<even if over>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWMC_SetMode(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!EW.g_CfgLoaded || command.ArgCount < 6) return;
			MCS_ValueMode Value = new MCS_ValueMode();
			if (!Int32.TryParse(command.GetArg(3), out Value.iMode) || Value.iMode < 0 ||  Value.iMode > 8) return;
			if (!Int32.TryParse(command.GetArg(4), out Value.iCooldown) || Value.iCooldown < 0) return;
			if (!Int32.TryParse(command.GetArg(5), out Value.iMaxuses) || Value.iMaxuses < 0) return;

			Value.bFlag = false;
			if (command.ArgCount >= 7)
			{
				if (!bool.TryParse(command.GetArg(6), out Value.bFlag))
					if (Int32.TryParse(command.GetArg(6), out int iFBuf) && iFBuf == 1) Value.bFlag = true;
			}

			MapCommands<MCS_ValueMode>.ForAllAbilities(command, MapCommands<MCS_ValueMode>.DG_SetMode, Value);
		}
		
		[ConsoleCommand("ew_lockbutton", "Allows to lock item")]
		[RequiresPermissions("@css/ew_map")]
		[CommandHelper(minArgs: 3, usage: "<hammerid> <buttonid> <value>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWMC_LockButton(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!EW.g_CfgLoaded || command.ArgCount < 4) return;

			bool bFlag = false;
			if (!bool.TryParse(command.GetArg(3), out bFlag))
				if (Int32.TryParse(command.GetArg(3), out int iFBuf) && iFBuf == 1) bFlag = true;

			MapCommands<bool>.ForAllAbilities(command, MapCommands<bool>.DG_LockButton, bFlag);
		}
		
		[ConsoleCommand("ew_setabilityname", "Allows you to change the ability’s name")]
		[RequiresPermissions("@css/ew_map")]
		[CommandHelper(minArgs: 3, usage: "<hammerid> <buttonid> <newname>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWMC_SetAbilityName(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!EW.g_CfgLoaded || command.ArgCount < 4) return;
			
			string sNewName = command.GetArg(3);
			if(string.IsNullOrEmpty(sNewName)) return;

			MapCommands<string>.ForAllAbilities(command, MapCommands<string>.DG_SetAbilityName, sNewName);
		}
		
		[ConsoleCommand("ew_setname", "Allows you to change the item’s name(Chat)")]
		[RequiresPermissions("@css/ew_map")]
		[CommandHelper(minArgs: 2, usage: "<hammerid> <newname>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWMC_SetFullName(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!EW.g_CfgLoaded || command.ArgCount < 3) return;
			
			string sNewName = command.GetArg(2);
			if(string.IsNullOrEmpty(sNewName)) return;

			MapCommands<string>.ForAllItems(command, MapCommands<string>.DG_SetFullName, sNewName);
		}
		
		[ConsoleCommand("ew_setshortname", "Allows you to change the item’s shortname(HUD)")]
		[RequiresPermissions("@css/ew_map")]
		[CommandHelper(minArgs: 2, usage: "<hammerid> <newshortname>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWMC_SetShortName(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!EW.g_CfgLoaded || command.ArgCount < 3) return;
			
			string sNewName = command.GetArg(2);
			if(string.IsNullOrEmpty(sNewName)) return;

			MapCommands<string>.ForAllItems(command, MapCommands<string>.DG_SetShortName, sNewName);
		}
		
		[ConsoleCommand("ew_block", "Allows you to block an item during the game. Similar to the 'blockpickup' property")]
		[RequiresPermissions("@css/ew_map")]
		[CommandHelper(minArgs: 2, usage: "<hammerid> <value>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
#nullable enable
		public void OnEWMC_Block(CCSPlayerController? player, CommandInfo command)
#nullable disable
		{
			if (!EW.g_CfgLoaded || command.ArgCount < 3) return;
			
			bool bFlag = false;
			if (!bool.TryParse(command.GetArg(2), out bFlag))
				if (Int32.TryParse(command.GetArg(2), out int iFBuf) && iFBuf == 1) bFlag = true;

			MapCommands<bool>.ForAllItems(command, MapCommands<bool>.DG_Block, bFlag);
		}
	}
}
