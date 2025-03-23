using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using EntWatchSharp.Helpers;
using EntWatchSharp.Items;

namespace EntWatchSharp.Modules
{
	static class SpawnItem
	{
		static CounterStrikeSharp.API.Modules.Utils.Vector vec = new (0, 0, 0);
		public static void Spawn(CCSPlayerController admin, CCSPlayerController receiver, string sItemName, bool bStrip, bool bConsole)
		{
			if (receiver.Pawn.Value == null || !receiver.Pawn.Value.IsValid || receiver.Pawn.Value.AbsOrigin == null)
			{
				UI.EWReplyInfo(admin, "Reply.No_matching_client", bConsole);
				return;
			}
			int iCount = 0;
			ItemConfig Item = new();
			foreach (ItemConfig ItemTest in EW.g_ItemConfig.ToList())
			{
				if ((ItemTest.Name.Contains(sItemName, StringComparison.OrdinalIgnoreCase) || ItemTest.ShortName.Contains(sItemName, StringComparison.OrdinalIgnoreCase)) && ItemTest.SpawnerID > 0)
				{
					iCount++;
					Item = ItemTest;
				}
			}
			if (iCount < 1)
			{
				UI.EWReplyInfo(admin, "Reply.Spawn.NoItem", bConsole);
				return;
			}
			if (iCount > 1)
			{
				UI.EWReplyInfo(admin, "Reply.Spawn.ManyItems", bConsole);
				foreach (ItemConfig ItemTest in EW.g_ItemConfig.ToList())
				{
					if ((ItemTest.Name.Contains(sItemName, StringComparison.OrdinalIgnoreCase) || ItemTest.ShortName.Contains(sItemName, StringComparison.OrdinalIgnoreCase)) && ItemTest.SpawnerID > 0)
					{
						UI.EWReplyInfo(admin, $"~{ItemTest.Name} ({ItemTest.ShortName})", bConsole);
					}
				}
				return;
			}
			if(Item.SpawnerID == 0)
			{
				UI.EWReplyInfo(admin, "Reply.Spawn.NoCfgSpawner", bConsole);
				return;
			}

			var entPTs = Utilities.FindAllEntitiesByDesignerName<CPointTemplate>("point_template");
			CPointTemplate entPT = null;
			foreach (var entity in entPTs)
			{
				if (entity != null && Int32.Parse(entity.UniqueHammerID) == Item.SpawnerID)
				{
					entPT = entity;
					break;
				}
			}
			if (entPT != null && entPT.IsValid && entPT.Entity != null)
			{
				if (bStrip) receiver.RemoveWeapons();
				vec.X = receiver.Pawn.Value.AbsOrigin.X;
				vec.Y = receiver.Pawn.Value.AbsOrigin.Y;
				vec.Z = receiver.Pawn.Value.AbsOrigin.Z + 20;
				
				Utilities.GetPlayers().ForEach(player =>
				{
					if (player != null && player.IsValid && player.Pawn.Value != null && player.Pawn.Value.IsValid && EW.CheckDictionary(player) && EW.Distance(vec, player.Pawn.Value.AbsOrigin) <= 64.0) EW.g_EWPlayer[player].BannedPlayer.bFixSpawnItem = true;
				});

				CEnvEntityMaker Maker = Utilities.CreateEntityByName<CEnvEntityMaker>("env_entity_maker");
				if (Maker != null && Maker.IsValid)
				{
					Maker.Template = entPT.Entity.Name;
					Maker.Flags = 0;
					Maker.DispatchSpawn();
					Maker.Teleport(vec);
					Maker.AcceptInput("ForceSpawn");
					Maker.Remove();
				}
				_ = new CounterStrikeSharp.API.Modules.Timers.Timer(0.2f, () =>
				{
					Utilities.GetPlayers().ForEach(player =>
					{
						if (player != null && player.IsValid && EW.CheckDictionary(player)) EW.g_EWPlayer[player].BannedPlayer.bFixSpawnItem = false;
					});
				});
				UI.EWChatAdmin("Reply.Spawn.Notify", $"{UI.PlayerInfo(admin)}{EW.g_Scheme.color_warning}", $"{Item.Color}{Item.Name}({Item.ShortName}){EW.g_Scheme.color_warning}", $"{UI.PlayerInfo(receiver)}");
				EW.g_cAPI?.OnAdminSpawnItem(admin, Item.Name, receiver);
			} else
			{
				UI.EWReplyInfo(admin, "Reply.Spawn.NoSpawner", bConsole);
				return;
			}
		}
	}
}
