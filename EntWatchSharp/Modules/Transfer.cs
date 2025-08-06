﻿using CounterStrikeSharp.API.Core;
using EntWatchSharp.Helpers;
using EntWatchSharp.Items;

namespace EntWatchSharp.Modules
{
	static class Transfer
	{
		public static void Target(CCSPlayerController admin, CCSPlayerController target, CCSPlayerController receiver, bool bConsole)
		{
			int iCount = 0;
			foreach (Item ItemTest in EW.g_ItemList)
			{
				if (ItemTest.Owner == target)
				{
					ItemName(admin, ItemTest, receiver, bConsole);
					iCount++;
				}
			}
			if (iCount == 0)
			{
				UI.EWReplyInfo(admin, "Reply.Transfer.NoItem", bConsole);
			}
		}

		public static void ItemName(CCSPlayerController admin, Item ItemTest, CCSPlayerController receiver, bool bConsole)
		{
			if (receiver.Pawn.Value == null || !receiver.Pawn.Value.IsValid || receiver.Pawn.Value.AbsOrigin == null)
			{
				UI.EWReplyInfo(admin, "Reply.No_matching_client", bConsole);
				return;
			}
			if (ItemTest.AllowTransfer != true)
			{
				UI.EWReplyInfo(admin, "Reply.Transfer.NotAllow", bConsole);
				return;
			}
			//Drop Weapon from Receiver
			foreach (var weapon in receiver!.PlayerPawn.Value!.WeaponServices!.MyWeapons)
			{
				if (!weapon.IsValid) continue;

				if (new CCSWeaponBaseVData(weapon.Value!.VData!.Handle)!.GearSlot == ItemTest.WeaponHandle.VData!.GearSlot)
				{
					CCSWeaponBase CheckWeapon = new(weapon.Value.Handle);
					if (CheckWeapon.IsValid)
					{
						foreach (Item ItemCheck in EW.g_ItemList.ToList())
						{
							//Console.WriteLine($"CheckWeapon:{CheckWeapon.Handle}/ItemCheck:{ItemCheck.Name}/ItemHandle:{ItemCheck.WeaponHandle.Handle}");
							if (CheckWeapon == ItemCheck.WeaponHandle)
							{
								UI.EWReplyInfo(admin, "Reply.Transfer.AlreadySlot", bConsole);
								return;
							}
						}
					}
					receiver.PlayerPawn.Value.WeaponServices.ActiveWeapon.Raw = weapon.Raw;
					receiver.DropActiveWeapon();
				}
			}
			CCSPlayerController target = null;
			if (ItemTest.Owner != null)
			{
				target = new CCSPlayerController(ItemTest.Owner.Handle);
				//Drop Weapon from Target
				foreach (var weapon in target!.PlayerPawn.Value!.WeaponServices!.MyWeapons)
				{
					if (!weapon.IsValid) continue;

					if (new CCSWeaponBaseVData(weapon.Value!.VData!.Handle)!.GearSlot == ItemTest.WeaponHandle.VData!.GearSlot)
					{
						target.PlayerPawn.Value.WeaponServices.ActiveWeapon.Raw = weapon.Raw;
						target.DropActiveWeapon();
					}
				}
				ItemTest.Owner = null;
				//fix bug drop
				_ = new CounterStrikeSharp.API.Modules.Timers.Timer(0.2f, () =>
				{
					try
					{
						if (ItemTest != null && ItemTest.WeaponHandle.IsValid) ItemTest.WeaponHandle.Teleport((System.Numerics.Vector3)receiver.Pawn.Value.AbsOrigin, null, null);
					}
					catch (Exception) { }
				});
			}
			ItemTest.WeaponHandle.Teleport((System.Numerics.Vector3)receiver.Pawn.Value.AbsOrigin, null, null);

			UI.EWChatAdminTransfer(UI.PlayerInfoFormat(admin), UI.PlayerInfoFormat(receiver), $"{ItemTest.Color}{ItemTest.Name}{EW.g_Scheme.color_warning}", target != null ? UI.PlayerInfoFormat(target) : UI.PlayerInfoFormat("Console", "Server"));
			EW.g_cAPI?.OnAdminTransferedItem(admin, ItemTest.Name, receiver);
		}
	}
}
