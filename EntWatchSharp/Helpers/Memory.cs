﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using System.Runtime.CompilerServices;

namespace EntWatchSharp
{
	public partial class EntWatchSharp : BasePlugin
	{
		static readonly MemoryFunctionVoid<nint, CPlayer_WeaponServices, CBaseEntity, IntPtr> CPlayer_WeaponServices_WeaponDropFunc = new (GameData.GetSignature("CPlayer_WeaponServices_WeaponDrop"));
		static readonly MemoryFunctionVoid<CEntityIdentity, CUtlSymbolLarge, CEntityInstance, CEntityInstance, CVariant, int> CEntityIdentity_AcceptInputFunc = new(GameData.GetSignature("CEntityIdentity_AcceptInput"));

		public void VirtualFunctionsInitialize()
		{
			//VirtualFunctions.CCSPlayer_WeaponServices_CanUseFunc.Hook(OnWeaponCanUse, HookMode.Pre);
			VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Hook(OnWeaponCanAcquire, HookMode.Pre);
			VirtualFunctions.CBaseTrigger_StartTouchFunc.Hook(OnTriggerStartTouch, HookMode.Pre);
			CPlayer_WeaponServices_WeaponDropFunc.Hook(OnWeaponDrop, HookMode.Post);
			CEntityIdentity_AcceptInputFunc.Hook(OnInput, HookMode.Pre);
		}

		public void VirtualFunctionsUninitialize()
		{
			//VirtualFunctions.CCSPlayer_WeaponServices_CanUseFunc.Unhook(OnWeaponCanUse, HookMode.Pre);
			VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Unhook(OnWeaponCanAcquire, HookMode.Pre);
			VirtualFunctions.CBaseTrigger_StartTouchFunc.Unhook(OnTriggerStartTouch, HookMode.Pre);
			CPlayer_WeaponServices_WeaponDropFunc.Unhook(OnWeaponDrop, HookMode.Post);
			CEntityIdentity_AcceptInputFunc.Unhook(OnInput, HookMode.Pre);
		}

		public static float MathCounter_GetValue(CMathCounter cMath)
		{
			return new CEntityOutputTemplate_float(cMath.Handle + Schema.GetSchemaOffset("CMathCounter", "m_OutValue")).OutValue;
			//return new CEntityOutputTemplate_float(cMath.Handle + 1264).OutValue;
		}
	}

	public class CEntityOutputTemplate_float(IntPtr pointer) : NativeObject(pointer)
	{
		public unsafe float OutValue => Unsafe.Add(ref *(float*)Handle, 6);
	}
}
