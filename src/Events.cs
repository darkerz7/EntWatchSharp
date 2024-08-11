using CounterStrikeSharp.API.Core.Attributes.Registration;
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
			RegisterEventHandler<EventRoundStart>(OnEventRoundStart);
			RegisterEventHandler<EventRoundEnd>(OnEventRoundEnd);
			RegisterEventHandler<EventItemPickup>(OnEventItemPickupPost);
			//OnWeaponCanUse
			//OnWeaponDrop
			RegisterEventHandler<EventPlayerDeath>(OnEventPlayerDeathPost);
			RegisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
			RegisterEventHandler<EventPlayerDisconnect>(OnEventPlayerDisconnect);
			RegisterEventHandler<EventPlayerSpawn>(OnEventPlayerSpawn);

			HookEntityOutput("func_button", "OnPressed", OnButtonPressed, HookMode.Pre);
			HookEntityOutput("func_rot_button", "OnPressed", OnButtonPressed, HookMode.Pre);
			HookEntityOutput("func_door", "OnOpen", OnButtonPressed, HookMode.Pre);
			HookEntityOutput("func_door_rotating", "OnOpen", OnButtonPressed, HookMode.Pre);
		}

		public void UnRegEvents()
		{
			RemoveListener<OnServerPrecacheResources>(OnPrecacheResources);
			RemoveListener<OnMapStart>(OnMapStart_Listener);
			RemoveListener<OnMapEnd>(OnMapEnd_Listener);
			RemoveListener<OnEntitySpawned>(OnEntitySpawned_Listener);
			RemoveListener<OnEntityDeleted>(OnEntityDeleted_Listener);
			RemoveListener<OnTick>(OnOnTick_Listener);
			DeregisterEventHandler<EventRoundStart>(OnEventRoundStart);
			DeregisterEventHandler<EventRoundEnd>(OnEventRoundEnd);
			DeregisterEventHandler<EventItemPickup>(OnEventItemPickupPost);
			DeregisterEventHandler<EventPlayerDeath>(OnEventPlayerDeathPost);
			DeregisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
			DeregisterEventHandler<EventPlayerDisconnect>(OnEventPlayerDisconnect);
			DeregisterEventHandler<EventPlayerSpawn>(OnEventPlayerSpawn);

			UnhookEntityOutput("func_button", "OnPressed", OnButtonPressed, HookMode.Pre);
			UnhookEntityOutput("func_rot_button", "OnPressed", OnButtonPressed, HookMode.Pre);
			UnhookEntityOutput("func_door", "OnOpen", OnButtonPressed, HookMode.Pre);
			UnhookEntityOutput("func_door_rotating", "OnOpen", OnButtonPressed, HookMode.Pre);
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
		}

		private void TimerRetry()
		{
			//Reban after reload plugin
			if (EbanDB.db.bDBReady)
			{
				Utilities.GetPlayers().ForEach(player =>
				{
					if (player.IsValid && EW.CheckDictionary(player, EW.g_BannedPlayer))
					{
						Server.NextFrame(async () =>
						{
							await EW.g_BannedPlayer[player].GetBan(player);
						});
					}
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

			int iTime = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

			Server.NextFrame(async () =>
			{
				await EbanDB.OfflineUnban(sServerName, iTime);
			});

			//Update (Un)Bans
			Utilities.GetPlayers().ForEach(player =>
			{
				if (player.IsValid && EW.CheckDictionary(player, EW.g_BannedPlayer))
				{
					Server.NextFrame(async () =>
					{
						if (!await EW.g_BannedPlayer[player].GetBan(player)) EW.g_BannedPlayer[player].bBanned = false;
					});
				}
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
			if (!entity.IsValid) return;
			if (entity.DesignerName.Contains("weapon_"))
			{
				Server.NextFrame(() =>
				{
					if (!EW.g_CfgLoaded) return;

					EW.WeaponIsItem(entity);
				});
			}
			else if (entity.DesignerName.CompareTo("func_button") == 0 || entity.DesignerName.CompareTo("func_rot_button") == 0 ||
				entity.DesignerName.CompareTo("func_door") == 0 || entity.DesignerName.CompareTo("func_door_rotating") == 0)
			{
				Server.NextFrame(() =>
				{
					if (!EW.g_CfgLoaded) return;

					var weapon = EW.EntityParentRecursive(entity);
					if (weapon != null && weapon.IsValid)
					{
						int iButtonID = 0;
						if (entity.DesignerName.CompareTo("func_button") == 0 || entity.DesignerName.CompareTo("func_rot_button") == 0)
							iButtonID = Int32.Parse(new CBaseButton(entity.Handle).UniqueHammerID);
						else
						{
							var cDoor = new CBasePropDoor(entity.Handle);
							if ((cDoor.Spawnflags & 256) != 1) return;
							iButtonID = Int32.Parse(cDoor.UniqueHammerID);
						}
						foreach (Item ItemTest in EW.g_ItemList.ToList())
						{
							if (weapon.Index == ItemTest.WeaponHandle.Index)
							{
								foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
								{
									if (AbilityTest.ButtonID == iButtonID || AbilityTest.ButtonID == 0)
									{
										AbilityTest.Entity = entity;
										AbilityTest.ButtonID = iButtonID;
										AbilityTest.ButtonClass = entity.DesignerName;
										return;
									}
								}
								Ability abilitytest = new Ability("", entity.DesignerName, true, 0, 0, 0, iButtonID, entity);
								ItemTest.AbilityList.Add(abilitytest);
							}
						}
					}
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
			else if (entity.DesignerName.CompareTo("func_button") == 0 || entity.DesignerName.CompareTo("func_rot_button") == 0 ||
				entity.DesignerName.CompareTo("func_door") == 0 || entity.DesignerName.CompareTo("func_door_rotating") == 0)
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
								if (EW.CheckDictionary(ItemTest.Owner, EW.g_UsePriorityPlayer)) EW.g_UsePriorityPlayer[ItemTest.Owner].UpdateCountButton();
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
				if (!EW.CheckDictionary(player, EW.g_UsePriorityPlayer)) return;
				EW.g_UsePriorityPlayer[player].DetectUse();
			});
		}

		[GameEventHandler]
		private HookResult OnEventRoundStart(EventRoundStart @event, GameEventInfo info)
		{
			EW.g_ItemList.Clear();
			Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false, PawnIsAlive: true }).ToList().ForEach(player =>
			{
				if (EW.CheckDictionary(player, EW.g_UsePriorityPlayer)) EW.g_UsePriorityPlayer[player].UpdateCountButton();
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
			if (!EW.g_CfgLoaded) return HookResult.Continue;

			CCSPlayerController pl = new CCSPlayerController(@event.Userid.Handle);

			if (pl.IsValid)
			{
				foreach (var ownerWeapon in pl.PlayerPawn.Value!.WeaponServices!.MyWeapons)
				{
					if (ownerWeapon is not { IsValid: true, Value.IsValid: true })
						continue;
					if (ownerWeapon.Value.AttributeManager.Item.ItemDefinitionIndex != @event.Defindex)
						continue;
					int indexItem = EW.IndexItem(ownerWeapon.Index);
					if (indexItem > -1)
					{
						EW.g_ItemList[indexItem].Owner = pl;
						EW.g_ItemList[indexItem].Team = pl.TeamNum;
						EW.g_ItemList[indexItem].SetDelay();
						if (EW.CheckDictionary(pl, EW.g_UsePriorityPlayer)) EW.g_UsePriorityPlayer[pl].UpdateCountButton();
						UI.EWChatActivity("Chat.Pickup", EW.g_Scheme.color_pickup, EW.g_ItemList[indexItem], EW.g_ItemList[indexItem].Owner);
						return HookResult.Continue;
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
				var client = new CCSPlayerController(hook.GetParam<CCSPlayer_WeaponServices>(0).Pawn.Value.Controller.Value.Handle);
				var weapon = hook.GetParam<CBasePlayerWeapon>(1);

				foreach(Item ItemTest in EW.g_ItemList.ToList())
				{
					if (ItemTest.WeaponHandle == weapon && (Cvar.BlockEPickup && (client.Buttons & PlayerButtons.Use) != 0 || (EW.CheckDictionary(client, EW.g_BannedPlayer) && EW.g_BannedPlayer[client].bBanned)))
					{
						hook.SetReturn(false);
						return HookResult.Handled;
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
				var client = new CCSPlayerController(hook.GetParam<CCSPlayer_WeaponServices>(0).Pawn.Value.Controller.Value.Handle);
				var weapon = hook.GetParam<CBasePlayerWeapon>(1);

				//Before death the hook is triggered
				Server.NextFrame(() =>
				{
					foreach(Item ItemTest in EW.g_ItemList.ToList())
					{
						if (ItemTest.WeaponHandle == weapon)
						{
							if (ItemTest.Owner == client)
							{
								ItemTest.Owner = null;
								if (EW.CheckDictionary(client, EW.g_UsePriorityPlayer)) EW.g_UsePriorityPlayer[client].UpdateCountButton();
								UI.EWChatActivity("Chat.Drop", EW.g_Scheme.color_drop, ItemTest, client);
							}
							return;
						}
					}
				});
			}catch (Exception) { }
			return HookResult.Continue;
		}

		[GameEventHandler(mode: HookMode.Post)]
		private HookResult OnEventPlayerDeathPost(EventPlayerDeath @event, GameEventInfo info)
		{
			if (!EW.g_CfgLoaded) return HookResult.Continue;

			CCSPlayerController pl = new CCSPlayerController(@event.Userid.Handle);

			if (pl.IsValid)
			{
				if (EW.CheckDictionary(pl, EW.g_HudPlayer))
				{
					EW.RemoveEntityHud(pl);
				}

				foreach(Item ItemTest in EW.g_ItemList.ToList())
				{
					if (ItemTest.Owner == pl)
					{
						ItemTest.Owner = null;
						if (EW.CheckDictionary(pl, EW.g_UsePriorityPlayer)) EW.g_UsePriorityPlayer[pl].UpdateCountButton();
						UI.EWChatActivity("Chat.Death", EW.g_Scheme.color_death, ItemTest, pl);
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
			if (!EW.g_CfgLoaded) return HookResult.Continue;

			CCSPlayerController pl = new CCSPlayerController(@event.Userid.Handle);

			if (pl.IsValid)
			{
				EW.CheckDictionary(pl, EW.g_HudPlayer); //Add HUD

				EW.CheckDictionary(pl, EW.g_UsePriorityPlayer); //Add UsePriority

				EW.LoadClientPrefs(pl);

				if(EW.CheckDictionary(pl, EW.g_BannedPlayer))  //Add Eban
				{
					Server.NextFrame(async () =>
					{
						await EW.g_BannedPlayer[pl].GetBan(pl); //Set Eban
						Server.NextFrame(() =>
						{
							if (EW.g_BannedPlayer[pl].bBanned) UI.EWSysInfo("Info.Eban.PlayerConnect", 4, UI.PlayerInfo(pl), EW.g_BannedPlayer[pl].iDuration, EW.g_BannedPlayer[pl].iTimeStamp_Issued, UI.PlayerInfo(EW.g_BannedPlayer[pl].sAdminName, EW.g_BannedPlayer[pl].sAdminSteamID), EW.g_BannedPlayer[pl].sReason);
						});
					});
				}
			}

			return HookResult.Continue;
		}

		[GameEventHandler]
		private HookResult OnEventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
		{
			if (!EW.g_CfgLoaded) return HookResult.Continue;

			if (EW.g_HudPlayer.ContainsKey(@event.Userid))
				EW.g_HudPlayer.Remove(@event.Userid);   //Remove HUD

			if (EW.g_UsePriorityPlayer.ContainsKey(@event.Userid))
				EW.g_UsePriorityPlayer.Remove(@event.Userid);   //Remove UsePriority

			if (EW.g_BannedPlayer.ContainsKey(@event.Userid))
				EW.g_BannedPlayer.Remove(@event.Userid);   //Remove Eban

			foreach(Item ItemTest in EW.g_ItemList.ToList())
			{
				if (ItemTest.Owner == @event.Userid)
				{
					ItemTest.Owner = null;
					UI.EWChatActivity("Chat.Disconnect", EW.g_Scheme.color_disconnect, ItemTest, @event.Userid);
					if (!ItemTest.ForceDrop)
					{
						ItemTest.WeaponHandle.Remove();
					}
				}
			}
			return HookResult.Continue;
		}

		[GameEventHandler(mode: HookMode.Post)]
		private HookResult OnEventPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
		{
			if (!EW.g_CfgLoaded) return HookResult.Continue;
			try
			{
				if (@event.Userid.IsValid)
				{
					if(EW.CheckDictionary(@event.Userid, EW.g_HudPlayer))
					{
						var plHud = EW.g_HudPlayer[@event.Userid];
						if (plHud is HudWorldText) ((HudWorldText)plHud).CreateHud();
					}
				}
			}catch (Exception) { }

			return HookResult.Continue;
		}

		//Crash after 3 reload plugin
		//[EntityOutputHook("func_button", "OnPressed")]
		//[EntityOutputHook("func_rot_button", "OnPressed")]
		//[EntityOutputHook("func_door", "OnOpen")]
		//[EntityOutputHook("func_door_rotating", "OnOpen")]
		public HookResult OnButtonPressed(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
		{
			if (!EW.g_CfgLoaded) return HookResult.Continue;
			try
			{
				if (!activator.IsValid || !caller.IsValid) return HookResult.Continue;

				EW.UpdateTime();
				foreach(Item ItemTest in EW.g_ItemList.ToList())
				{
					foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
					{
						if (AbilityTest.Entity != null && AbilityTest.Entity.IsValid && caller == AbilityTest.Entity)
						{
							if (ItemTest.Owner != null && ItemTest.Owner.IsValid && ItemTest.Owner.Pawn.IsValid && ItemTest.Owner.Pawn.Index == activator.Index && ItemTest.CheckDelay() && AbilityTest.Ready())
							{
								AbilityTest.Used();
								UI.EWChatActivity("Chat.Use", EW.g_Scheme.color_use, ItemTest, ItemTest.Owner, AbilityTest);
								return HookResult.Continue;
							}
							else return HookResult.Handled;
						}
					}
				}
			}catch (Exception) { }
			return HookResult.Continue;
		}
		private HookResult OnInput(DynamicHook hook)
		{
			if (!EW.g_CfgLoaded) return HookResult.Continue;
			//var cEntity = hook.GetParam<CEntityIdentity>(0);
			var cInput = hook.GetParam<CUtlSymbolLarge>(1);
			var cActivator = hook.GetParam<CEntityInstance>(2);
			var cCaller = hook.GetParam<CEntityInstance>(3);
			if (cActivator == null || !cActivator.IsValid || !EW.IsGameUI(cCaller) || cInput.KeyValue.ToLower().CompareTo("invalue") != 0) return HookResult.Continue;
			var cValue = new CUtlSymbolLarge(hook.GetParam<CVariant>(4).Handle);

			EW.UpdateTime();
			foreach (Item ItemTest in EW.g_ItemList.ToList())
			{
				foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
				{
					if (AbilityTest.ButtonClass.ToLower().StartsWith("game_ui::"))
					{
						if (AbilityTest.ButtonClass.ToLower().Substring(9).CompareTo(cValue.KeyValue.ToLower()) == 0)
						{
							if (ItemTest.Owner != null && ItemTest.Owner.IsValid && ItemTest.Owner.Pawn.IsValid && ItemTest.Owner.Pawn.Index == cActivator.Index && ItemTest.CheckDelay() && AbilityTest.Ready())
							{
								AbilityTest.Used();
								UI.EWChatActivity("Chat.Use", EW.g_Scheme.color_use, ItemTest, ItemTest.Owner, AbilityTest);
								return HookResult.Continue;
							} else return HookResult.Handled;
						}
					}
				}
			}
			return HookResult.Continue;
		}
			private HookResult OnGameUI(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
		{
			if (!EW.g_CfgLoaded) return HookResult.Continue;
			try
			{
				
			}
			catch (Exception) { }
			return HookResult.Continue;
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

				if(Cvar.TriggerOnceException && trigger.DesignerName.CompareTo("trigger_once") == 0) return HookResult.Continue; //Outputs don't work but trigger disappears

				//Console.WriteLine($"Player: {player.PlayerName} OnStartTouch: {trigger.Entity.Name}[HID:{trigger.UniqueHammerID}|ID:{trigger.Index}]");

				if (!EW.CheckDictionary(player, EW.g_BannedPlayer)) return HookResult.Continue;
				if (EW.g_BannedPlayer[player].bBanned)
				{
					foreach (ItemConfig ItemTest in EW.g_ItemConfig.ToList())
					{
						if (ItemTest.TriggerID > 0 && ItemTest.TriggerID.ToString().CompareTo(trigger.UniqueHammerID) == 0)
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
