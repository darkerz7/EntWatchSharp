﻿using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Timers;
using static CounterStrikeSharp.API.Core.Listeners;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using EntWatchSharp.Items;
using EntWatchSharp.Helpers;
using CounterStrikeSharp.API.Modules.Utils;
using EntWatchSharp.Modules.Eban;
using EntWatchSharp.Modules;

namespace EntWatchSharp
{
    public partial class EntWatchSharp : BasePlugin
	{
		public void RegEvents()
		{
			RegisterListener<OnServerPrecacheResources>(OnPrecacheResources);
			RegisterListener<OnMapStart>(OnMapStart_Listener);
			RegisterListener<OnMapEnd>(OnMapEnd_Listener);
			RegisterListener<OnEntitySpawned>(OnEntitySpawned_Listener);
			RegisterListener<OnEntityDeleted>(OnEntityDeleted_Listener);
			RegisterListener<OnTick>(OnOnTick_Listener);
			RegisterListener<CheckTransmit>(OnCheckTransmit_Listener);
			RegisterEventHandler<EventRoundStart>(OnEventRoundStart);
			RegisterEventHandler<EventRoundEnd>(OnEventRoundEnd);
			RegisterEventHandler<EventItemPickup>(OnEventItemPickupPost);
			//OnWeaponCanUse
			//OnWeaponDrop
			RegisterEventHandler<EventPlayerDeath>(OnEventPlayerDeathPost);
			RegisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
			RegisterEventHandler<EventPlayerDisconnect>(OnEventPlayerDisconnect);

			//Garbage collector crashes when reloading plugin on these hooks
			/*HookEntityOutput("func_button", "OnPressed", OnButtonPressed, HookMode.Pre);
			HookEntityOutput("func_rot_button", "OnPressed", OnButtonPressed, HookMode.Pre);
			HookEntityOutput("func_door", "OnOpen", OnButtonPressed, HookMode.Pre);
			HookEntityOutput("func_door_rotating", "OnOpen", OnButtonPressed, HookMode.Pre);
			HookEntityOutput("func_physbox", "OnPlayerUse", OnButtonPressed, HookMode.Pre);*/

			HookEntityOutput("func_button", "OnPressed", (_, _, activator, caller, _, _) =>
			{
				if (!OnButtonPressed(activator, caller)) return HookResult.Handled;
				return HookResult.Continue;
			});
			HookEntityOutput("func_rot_button", "OnPressed", (_, _, activator, caller, _, _) =>
			{
				if (!OnButtonPressed(activator, caller)) return HookResult.Handled;
				return HookResult.Continue;
			});
			HookEntityOutput("func_door", "OnOpen", (_, _, activator, caller, _, _) =>
			{
				if (!OnButtonPressed(activator, caller)) return HookResult.Handled;
				return HookResult.Continue;
			});
			HookEntityOutput("func_door_rotating", "OnOpen", (_, _, activator, caller, _, _) =>
			{
				if (!OnButtonPressed(activator, caller)) return HookResult.Handled;
				return HookResult.Continue;
			});
			HookEntityOutput("func_physbox", "OnPlayerUse", (_, _, activator, caller, _, _) =>
			{
				if (!OnButtonPressed(activator, caller)) return HookResult.Handled;
				return HookResult.Continue;
			});
		}

		public void UnRegEvents()
		{
			RemoveListener<OnServerPrecacheResources>(OnPrecacheResources);
			RemoveListener<OnMapStart>(OnMapStart_Listener);
			RemoveListener<OnMapEnd>(OnMapEnd_Listener);
			RemoveListener<OnEntitySpawned>(OnEntitySpawned_Listener);
			RemoveListener<OnEntityDeleted>(OnEntityDeleted_Listener);
			RemoveListener<OnTick>(OnOnTick_Listener);
			RemoveListener<CheckTransmit>(OnCheckTransmit_Listener);
			DeregisterEventHandler<EventRoundStart>(OnEventRoundStart);
			DeregisterEventHandler<EventRoundEnd>(OnEventRoundEnd);
			DeregisterEventHandler<EventItemPickup>(OnEventItemPickupPost);
			DeregisterEventHandler<EventPlayerDeath>(OnEventPlayerDeathPost);
			DeregisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
			DeregisterEventHandler<EventPlayerDisconnect>(OnEventPlayerDisconnect);

			/*UnhookEntityOutput("func_button", "OnPressed", OnButtonPressed, HookMode.Pre);
			UnhookEntityOutput("func_rot_button", "OnPressed", OnButtonPressed, HookMode.Pre);
			UnhookEntityOutput("func_door", "OnOpen", OnButtonPressed, HookMode.Pre);
			UnhookEntityOutput("func_door_rotating", "OnOpen", OnButtonPressed, HookMode.Pre);
			UnhookEntityOutput("func_physbox", "OnPlayerUse", OnButtonPressed, HookMode.Pre);*/
		}

		private void OnPrecacheResources(ResourceManifest manifest)
		{
			manifest.AddResource("particles/overhead_icon_fx/player_ping_ground_rings.vpcf");
		}

		private void OnMapStart_Listener(string sMapName)
		{
			EW.CleanData();
			EW.LoadScheme();
			EW.LoadConfig();
			if (EW.g_Timer != null)
			{
				EW.g_Timer.Kill();
				EW.g_Timer = null;
			}
			EW.g_Timer = new CounterStrikeSharp.API.Modules.Timers.Timer(1.0f, TimerUpdate, TimerFlags.REPEAT);
			LogManager.SystemAction("Info.ChangeMap", sMapName);
		}

		private void TimerUpdate()
		{
			if (!EW.g_CfgLoaded) return;

			EW.ShowHud();
			ClanTag.UpdateClanTag();
		}

		private void TimerRetry()
		{
			//Reban after reload plugin
			if (EbanDB.db.bDBReady)
			{
				Utilities.GetPlayers().ForEach(player =>
				{
					EbanPlayer.GetBan(player);
				});
				if (EW.g_TimerRetryDB != null)
				{
					EW.g_TimerRetryDB.Kill();
					EW.g_TimerRetryDB = null;
				}
			}
		}

		private void TimerUnban()
		{
			string sServerName = EW.g_Scheme.server_name;
			if (!string.IsNullOrEmpty(sServerName)) { sServerName = "Zombies Server"; }
			
			EbanDB.OfflineUnban(sServerName);

			Task.Run(() =>
			{
				OfflineFunc.TimeToClear();
			});

			//Update (Un)Bans
			Utilities.GetPlayers().ForEach(player =>
			{
				EbanPlayer.GetBan(player);
			});
		}

		private void OnMapEnd_Listener()
		{
			EW.CleanData();
			if (EW.g_Timer != null)
			{
				EW.g_Timer.Kill();
				EW.g_Timer = null;
			}
		}

		private void OnEntitySpawned_Listener(CEntityInstance entity)
		{
			if (!EW.g_CfgLoaded) return;
			if (entity == null || !entity.IsValid) return;
			if (entity.DesignerName.Contains("weapon_"))
			{
				Server.NextFrame(() =>
				{
					EW.WeaponIsItem(entity);
				});
			}
			else if (string.Equals(entity.DesignerName, "func_button") || string.Equals(entity.DesignerName, "func_rot_button") ||
				string.Equals(entity.DesignerName, "func_physbox") || string.Equals(entity.DesignerName, "func_door") || string.Equals(entity.DesignerName, "func_door_rotating"))
			{
				Server.NextFrame(() =>
				{
					var weapon = EW.EntityParentRecursive(entity);
					if (weapon != null && weapon.IsValid)
					{
						string sButtonID = "";
						if (string.Equals(entity.DesignerName, "func_button") || string.Equals(entity.DesignerName, "func_rot_button"))
							sButtonID = new CBaseButton(entity.Handle).UniqueHammerID;
						else if(string.Equals(entity.DesignerName, "func_physbox"))
						{
							sButtonID = new CPhysBox(entity.Handle).UniqueHammerID;
						}
						else 
						{
							var cDoor = new CBasePropDoor(entity.Handle);
							if ((cDoor.Spawnflags & 256) != 1) return;
							sButtonID = cDoor.UniqueHammerID;
						}
						foreach (Item ItemTest in EW.g_ItemList.ToList())
						{
							if (weapon.Index == ItemTest.WeaponHandle.Index)
							{
								foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
								{
									if (string.Equals(AbilityTest.ButtonID, sButtonID) || string.IsNullOrEmpty(AbilityTest.ButtonID) || string.Equals(AbilityTest.ButtonID, "0"))
									{
										AbilityTest.Entity = entity;
										AbilityTest.ButtonID = sButtonID;
										AbilityTest.ButtonClass = entity.DesignerName;
										return;
									}
								}
								Ability abilitytest = new("", entity.DesignerName, true, 0, 0, 0, sButtonID, entity);
								ItemTest.AbilityList.Add(abilitytest);
							}
						}
					}
				});
			} else if(string.Equals(entity.DesignerName, "math_counter"))
			{
				Server.NextFrame(() =>
				{
					CMathCounter cMathCounter = new(entity.Handle); // Server NextFrame or Timer - Need tests??
					_ = new CounterStrikeSharp.API.Modules.Timers.Timer(2.0f, () =>
					{
						if (cMathCounter == null || !cMathCounter.IsValid || cMathCounter.Entity == null || string.IsNullOrEmpty(cMathCounter.Entity.Name)) return; //Bad math_counter
						foreach (Item ItemTest in EW.g_ItemList.ToList())
						{
							if (ItemTest.WeaponHandle == null || !ItemTest.WeaponHandle.IsValid || ItemTest.WeaponHandle.Entity == null) continue;
							foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
							{
								if (!AbilityTest.MathFindSpawned && !string.IsNullOrEmpty(AbilityTest.MathID) && !string.Equals(AbilityTest.MathID, "0") && string.Equals(AbilityTest.MathID, cMathCounter.UniqueHammerID))
								{
									if (AbilityTest.MathNameFix) // <objectname> + _ + <serial number from 1> example: weapon_fire_125
									{
										if (string.IsNullOrEmpty(ItemTest.WeaponHandle.Entity.Name)) continue;
										int iIndexWeapon = ItemTest.WeaponHandle.Entity.Name.LastIndexOf('_');
										if (iIndexWeapon == -1) continue;
										int iIndexMathCounter = cMathCounter.Entity.Name.LastIndexOf('_');
										if (iIndexMathCounter == -1) return; //Another math_counter or bad EW config
										string sFix = cMathCounter.Entity.Name[iIndexMathCounter..];
										if (!string.Equals(sFix, ItemTest.WeaponHandle.Entity.Name[iIndexWeapon..])) continue;
									}
									AbilityTest.MathCounter = cMathCounter;
									return;
								}
							}

						}

					});
				});
			}
		}

		private void OnEntityDeleted_Listener(CEntityInstance entity)
		{
			if (!EW.g_CfgLoaded) return;

			if (entity.DesignerName.Contains("weapon_"))
			{
				var weapon = new CBasePlayerWeapon(entity.Handle);
				Server.NextFrame(() =>
				{
					foreach (Item ItemTest in EW.g_ItemList.ToList())
					{
						if (ItemTest.WeaponHandle == weapon) EW.g_ItemList.Remove(ItemTest);
					}
				});
			}
			else if (string.Equals(entity.DesignerName, "func_button") || string.Equals(entity.DesignerName, "func_rot_button") ||
				string.Equals(entity.DesignerName, "func_physbox") || string.Equals(entity.DesignerName, "func_door") || string.Equals(entity.DesignerName, "func_door_rotating"))
			{
				Server.NextFrame(() =>
				{
					foreach (Item ItemTest in EW.g_ItemList.ToList())
					{
						foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
						{
							if (AbilityTest.Entity == entity)
							{
								ItemTest.AbilityList.Remove(AbilityTest);
								if (ItemTest.Owner != null && EW.CheckDictionary(ItemTest.Owner)) EW.g_EWPlayer[ItemTest.Owner].UsePriorityPlayer.UpdateCountButton(ItemTest.Owner);
							}
						}
					}
				});
			}else if (string.Equals(entity.DesignerName, "math_counter"))
			{
				CMathCounter cMathCounter = new(entity.Handle);
				Server.NextFrame(() =>
				{
					foreach (Item ItemTest in EW.g_ItemList.ToList())
					{
						foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
						{
							if (!string.IsNullOrEmpty(AbilityTest.MathID) && !string.Equals(AbilityTest.MathID, "0") && AbilityTest.MathCounter == cMathCounter)
							{
								ItemTest.AbilityList.Remove(AbilityTest);
								if (EW.CheckDictionary(ItemTest.Owner)) EW.g_EWPlayer[ItemTest.Owner].UsePriorityPlayer.UpdateCountButton(ItemTest.Owner);
							}
						}
					}
				});
			}
		}

		//use priority
		private void OnOnTick_Listener()
		{
			if (!EW.g_CfgLoaded || !Cvar.UsePriority) return;
			Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false, PawnIsAlive: true }).ToList().ForEach(player =>
			{
				if (!EW.CheckDictionary(player)) return;
				EW.g_EWPlayer[player].UsePriorityPlayer.DetectUse(player);
			});
		}

		private void OnCheckTransmit_Listener(CCheckTransmitInfoList infoList)
		{
			if (!EW.g_CfgLoaded) return;
#nullable enable
			foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
#nullable disable
			{
				if (player == null || !player.IsValid) continue;
				foreach (KeyValuePair<CCSPlayerController, EWPlayer> ewp in EW.g_EWPlayer)
				{
					/*if (ewp.Value.HudPlayer is HudWorldText hud && hud.Entity != null && hud.Entity.IsValid && player != ewp.Key)
					{
						info.TransmitEntities.Remove(hud.Entity);
					}*/
					if (Cvar.GlowVIP)
					{
						if(!ewp.Value.PrivilegePlayer.WeaponGlow)
						{
							foreach (Item ItemTest in EW.g_ItemList.ToList())
							{
								if (Cvar.GlowProp && ItemTest.Prop != null && ItemTest.Prop.IsValid) info.TransmitEntities.Remove(ItemTest.Prop);
								if (Cvar.GlowParticle && ItemTest.Particle != null && ItemTest.Particle.IsValid) info.TransmitEntities.Remove(ItemTest.Particle);
							}
						}
					}
				}
			}
		}

		[GameEventHandler]
		private HookResult OnEventRoundStart(EventRoundStart @event, GameEventInfo info)
		{
			EW.g_ItemList.Clear();
			Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false, PawnIsAlive: true }).ToList().ForEach(player =>
			{
				ClanTag.RemoveClanTag(player);
				if (EW.CheckDictionary(player)) EW.g_EWPlayer[player].UsePriorityPlayer.UpdateCountButton(player);
			});
			return HookResult.Continue;
		}

		[GameEventHandler]
		private HookResult OnEventRoundEnd(EventRoundEnd @event, GameEventInfo info)
		{
			Server.NextFrame(EW.g_ItemList.Clear);
			return HookResult.Continue;
		}

		[GameEventHandler(mode: HookMode.Post)]
		private HookResult OnEventItemPickupPost(EventItemPickup @event, GameEventInfo info)
		{
			if (!EW.g_CfgLoaded || @event.Userid == null) return HookResult.Continue;

			CCSPlayerController pl = new(@event.Userid.Handle);

			if (pl.IsValid)
			{
				foreach (var ownerWeapon in pl.PlayerPawn.Value!.WeaponServices!.MyWeapons)
				{
					if (ownerWeapon is not { IsValid: true, Value.IsValid: true })
						continue;
					if (ownerWeapon.Value.AttributeManager.Item.ItemDefinitionIndex != @event.Defindex)
						continue;

					foreach (Item ItemTest in EW.g_ItemList.ToList())
					{
						if (ItemTest.ThisItem(ownerWeapon.Index))
						{
							ItemTest.Owner = pl;
							ItemTest.Team = pl.TeamNum;
							ItemTest.SetDelay();
							if (EW.CheckDictionary(pl)) EW.g_EWPlayer[pl].UsePriorityPlayer.UpdateCountButton(pl);
							UI.EWChatActivity("Chat.Pickup", EW.g_Scheme.color_pickup, ItemTest, ItemTest.Owner);
							EW.g_cAPI?.OnPickUpItem(ItemTest.Name, pl);
							ClanTag.UpdatePickUp(ItemTest);
							ItemTest.DisableGlow();
							foreach (OfflineBan OfflineTest in EW.g_OfflinePlayer.ToList())
							{
								if (pl.UserId == OfflineTest.UserID)
								{
									OfflineTest.LastItem = ItemTest.Name;
									break;
								}
							}
							return HookResult.Continue;
						}
					}
				}
			}
			return HookResult.Continue;
		}

		private HookResult OnWeaponCanUse(DynamicHook hook)
		{
			if (!EW.g_CfgLoaded) return HookResult.Continue;

			try
			{
				var service = hook.GetParam<CCSPlayer_WeaponServices>(0);
				if (service.Pawn.Value.Controller.Value != null)
				{
					var client = new CCSPlayerController(service.Pawn.Value.Controller.Value.Handle);
					var weapon = hook.GetParam<CBasePlayerWeapon>(1);

					if (EW.CheckDictionary(client) && EW.g_EWPlayer[client].BannedPlayer.bFixSpawnItem)
					{
						hook.SetReturn(false);
						return HookResult.Handled;
					}

					foreach (Item ItemTest in EW.g_ItemList.ToList())
					{
						if (ItemTest.WeaponHandle == weapon && (Cvar.BlockEPickup && (client.Buttons & PlayerButtons.Use) != 0 || (EW.CheckDictionary(client) && EW.g_EWPlayer[client].BannedPlayer.bBanned)))
						{
							hook.SetReturn(false);
							return HookResult.Handled;
						}
					}
				}
			}
			catch (Exception) { }

			return HookResult.Continue;
		}

		private HookResult OnWeaponDrop(DynamicHook hook)
		{
			if (!EW.g_CfgLoaded) return HookResult.Continue;

			try
			{
				//crash on hook.GetParam<CCSPlayer_WeaponServices>(0).Pawn.Value
				var service = hook.GetParam<CCSPlayer_WeaponServices>(0);
				if (service.Pawn.Value.Controller.Value != null)
				{
					var client = new CCSPlayerController(service.Pawn.Value.Controller.Value.Handle);
					var weapon = hook.GetParam<CBasePlayerWeapon>(1);

					//Before death the hook is triggered
					Server.NextFrame(() =>
					{
						foreach (Item ItemTest in EW.g_ItemList.ToList())
						{
							if (ItemTest.WeaponHandle == weapon)
							{
								if (ItemTest.Owner == client)
								{
									ItemTest.Owner = null;
									if (EW.CheckDictionary(client)) EW.g_EWPlayer[client].UsePriorityPlayer.UpdateCountButton(client);
									UI.EWChatActivity("Chat.Drop", EW.g_Scheme.color_drop, ItemTest, client);
									EW.g_cAPI?.OnDropItem(ItemTest.Name, client);
									ClanTag.RemoveClanTag(client);
									ItemTest.EnableGlow();
								}
								return;
							}
						}
					});
				}
			}catch (Exception) { }
			return HookResult.Continue;
		}

		[GameEventHandler(mode: HookMode.Post)]
		private HookResult OnEventPlayerDeathPost(EventPlayerDeath @event, GameEventInfo info)
		{
			if (!EW.g_CfgLoaded || @event.Userid == null) return HookResult.Continue;

			CCSPlayerController pl = new(@event.Userid.Handle);

			if (pl.IsValid)
			{
				foreach(Item ItemTest in EW.g_ItemList.ToList())
				{
					if (ItemTest.Owner == pl)
					{
						ItemTest.Owner = null;
						if (EW.CheckDictionary(pl)) EW.g_EWPlayer[pl].UsePriorityPlayer.UpdateCountButton(pl);
						UI.EWChatActivity("Chat.Death", EW.g_Scheme.color_death, ItemTest, pl);
						EW.g_cAPI?.OnPlayerDeathWithItem(ItemTest.Name, pl);
						ClanTag.RemoveClanTag(pl);
						ItemTest.EnableGlow();
						if (!ItemTest.ForceDrop)
						{
							ItemTest.WeaponHandle.Remove();
						}
					}
				}
			}
			return HookResult.Continue;
		}

		[GameEventHandler]
		private HookResult OnEventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
		{
			if (@event.Userid == null) return HookResult.Continue;

			CCSPlayerController pl = new(@event.Userid.Handle);

			OfflineFunc.PlayerConnectFull(pl);

			if (!EW.g_CfgLoaded) return HookResult.Continue;

			if (pl.IsValid)
			{
				EW.CheckDictionary(pl); //Add EWPlayer

				EW.LoadClientPrefs(pl);
			}

			EbanPlayer.GetBan(pl, true); //Set Eban

			return HookResult.Continue;
		}

		[GameEventHandler]
		private HookResult OnEventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
		{
			if (@event.Userid == null) return HookResult.Continue;
			OfflineFunc.PlayerDisconnect(@event.Userid);

			if (!EW.g_CfgLoaded) return HookResult.Continue;

			if (EW.g_EWPlayer.ContainsKey(@event.Userid))
				EW.g_EWPlayer.Remove(@event.Userid);   //Remove EWPlayer

			foreach(Item ItemTest in EW.g_ItemList.ToList())
			{
				if (ItemTest.Owner == @event.Userid)
				{
					ItemTest.Owner = null;
					UI.EWChatActivity("Chat.Disconnect", EW.g_Scheme.color_disconnect, ItemTest, @event.Userid);
					EW.g_cAPI?.OnPlayerDisconnectWithItem(ItemTest.Name, @event.Userid);
					ClanTag.RemoveClanTag(@event.Userid);
					ItemTest.EnableGlow();
					if (!ItemTest.ForceDrop)
					{
						ItemTest.WeaponHandle.Remove();
					}
				}
			}
			return HookResult.Continue;
		}

		//Crash after 3 reload plugin
		//[EntityOutputHook("func_button", "OnPressed")]
		//[EntityOutputHook("func_rot_button", "OnPressed")]
		//[EntityOutputHook("func_door", "OnOpen")]
		//[EntityOutputHook("func_door_rotating", "OnOpen")]
		//public HookResult OnButtonPressed(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
		private static bool OnButtonPressed(CEntityInstance activator, CEntityInstance caller)
		{
			if (!EW.g_CfgLoaded) return true;
			try
			{
				if (activator == null || !activator.IsValid || caller == null || !caller.IsValid) return true;

				EW.UpdateTime();
				foreach (Item ItemTest in EW.g_ItemList.ToList())
				{
					foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
					{
						if (AbilityTest.Entity != null && AbilityTest.Entity.IsValid && caller == AbilityTest.Entity)
						{
							if (ItemTest.Owner != null && ItemTest.Owner.IsValid && ItemTest.Owner.Pawn.IsValid && ItemTest.Owner.Pawn.Index == activator.Index && ItemTest.CheckDelay() && AbilityTest.Ready())
							{
								AbilityTest.SetFilter(activator);
								AbilityTest.Used();
								UI.EWChatActivity("Chat.Use", EW.g_Scheme.color_use, ItemTest, ItemTest.Owner, AbilityTest);
								EW.g_cAPI?.OnUseItem(ItemTest.Name, ItemTest.Owner, AbilityTest.Name);
								return true;
							}
							else return false;
						}
					}
				}
			}
			catch (Exception) { }
			return true;
		}

		private HookResult OnInput(DynamicHook hook)
		{
			if (!EW.g_CfgLoaded) return HookResult.Continue;

			//var cEntity = hook.GetParam<CEntityIdentity>(0);
			var sInput = hook.GetParam<CUtlSymbolLarge>(1).String;
			if (string.IsNullOrEmpty(sInput)) return HookResult.Continue;
			var cActivator = hook.GetParam<CEntityInstance>(2);
			var cCaller = hook.GetParam<CEntityInstance>(3);
			//Fix func_physbox:OnPlayerUse begin
			/*if (cActivator == null || !cActivator.IsValid) return HookResult.Continue;
			if (string.Equals(cEntity?.DesignerName, "func_physbox"))
			{
				Console.WriteLine($"Input: cEntity - {cEntity.DesignerName} sInput - {sInput}");
				if(string.Equals(sInput.ToLower(), "use"))
				{
					if (!OnButtonPressed(cActivator, cEntity.EntityInstance)) return HookResult.Handled;
					return HookResult.Continue;
				}
				return HookResult.Continue;
			}
			if (!EW.IsGameUI(cCaller) || !string.Equals(sInput.ToLower(), "invalue")) return HookResult.Continue;*/
			//Fix func_physbox:OnPlayerUse end
			if (cActivator == null || !cActivator.IsValid || !EW.IsGameUI(cCaller) || !string.Equals(sInput.ToLower(), "invalue")) return HookResult.Continue;
			var cValue = hook.GetParam<CVariant>(4);
			var sValue = cValue.FieldType == fieldtype_t.FIELD_CSTRING ? NativeAPI.GetStringFromSymbolLarge(cValue.Handle) : "";

			EW.UpdateTime();
			foreach (Item ItemTest in EW.g_ItemList.ToList())
			{
				foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
				{
					if (AbilityTest.ButtonClass.StartsWith("game_ui::", StringComparison.OrdinalIgnoreCase))
					{
						if (string.Equals(AbilityTest.ButtonClass.ToLower()[9..], sValue.ToLower()))
						{
							if (ItemTest.Owner != null && ItemTest.Owner.IsValid && ItemTest.Owner.Pawn.IsValid && ItemTest.Owner.Pawn.Index == cActivator.Index && ItemTest.CheckDelay() && AbilityTest.Ready())
							{
								AbilityTest.SetFilter(cActivator);
								AbilityTest.Used();
								UI.EWChatActivity("Chat.Use", EW.g_Scheme.color_use, ItemTest, ItemTest.Owner, AbilityTest);
								EW.g_cAPI?.OnUseItem(ItemTest.Name, ItemTest.Owner, AbilityTest.Name);
								return HookResult.Continue;
							}
						}
					}
				}
			}
			return HookResult.Continue;
		}
#nullable enable
		public static CCSPlayerController? EntityIsPlayer(CEntityInstance? entity)
#nullable disable
		{
			if (entity != null && entity.IsValid && string.Equals(entity.DesignerName, "player"))
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
		private HookResult OnTriggerStartTouch(DynamicHook hook)
		{
			if (!EW.g_CfgLoaded) return HookResult.Continue;
			try
			{
				var entity = hook.GetParam<CBaseEntity>(1);
				if (!entity.IsValid || entity.DesignerName != "player") return HookResult.Continue;
				var player = new CCSPlayerController(new CCSPlayerPawn(entity.Handle).Controller.Value!.Handle);
				var trigger = hook.GetParam<CBaseTrigger>(0);

				if(!player.IsValid || !trigger.IsValid) return HookResult.Continue;

				if(Cvar.TriggerOnceException && string.Equals(trigger.DesignerName, "trigger_once")) return HookResult.Continue; //Outputs don't work but trigger disappears

				//Console.WriteLine($"Player: {player.PlayerName} OnStartTouch: {trigger.Entity.Name}[HID:{trigger.UniqueHammerID}|ID:{trigger.Index}]");

				if (!EW.CheckDictionary(player)) return HookResult.Continue;
				if (EW.g_EWPlayer[player].BannedPlayer.bBanned)
				{
					foreach (ItemConfig ItemTest in EW.g_ItemConfig.ToList())
					{
						if (!string.IsNullOrEmpty(ItemTest.TriggerID) && !string.Equals(ItemTest.TriggerID, "0") && string.Equals(ItemTest.TriggerID, trigger.UniqueHammerID))
						{
							return HookResult.Handled;
						}
					}
				}
			}
			catch (Exception) { }
			return HookResult.Continue;
		}
	}
}
