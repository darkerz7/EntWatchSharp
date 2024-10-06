using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using EntWatchSharp.Helpers;
using EntWatchSharp.Items;

namespace EntWatchSharp.Modules
{
	static class SpawnItem
	{
		public static void Spawn(CCSPlayerController admin, CCSPlayerController receiver, string sItemName, bool bStrip, bool bConsole)
		{
			int iCount = 0;
			ItemConfig Item = new ItemConfig();
			foreach (ItemConfig ItemTest in EW.g_ItemConfig.ToList())
			{
				if ((ItemTest.Name.ToLower().Contains(sItemName) || ItemTest.ShortName.ToLower().Contains(sItemName)) && ItemTest.Spawner > 0)
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
					if ((ItemTest.Name.ToLower().Contains(sItemName) || ItemTest.ShortName.ToLower().Contains(sItemName)) && ItemTest.Spawner > 0)
					{
						UI.EWReplyInfo(admin, $"~{ItemTest.Name} ({ItemTest.ShortName})", bConsole);
					}
				}
				return;
			}
			if(Item.Spawner == 0)
			{
				UI.EWReplyInfo(admin, "Reply.Spawn.NoCfgSpawner", bConsole);
				return;
			}

			var entPTs = Utilities.FindAllEntitiesByDesignerName<CPointTemplate>("point_template");
			CPointTemplate entPT = null;
			foreach (var entity in entPTs)
			{
				if (entity != null && Int32.Parse(entity.UniqueHammerID) == Item.Spawner)
				{
					entPT = entity;
					break;
				}
			}
			if (entPT != null)
			{
				if (bStrip) receiver.RemoveWeapons();
				if (EW.CheckDictionary(receiver, EW.g_BannedPlayer)) EW.g_BannedPlayer[receiver].bFixSpawnItem = true;
				CounterStrikeSharp.API.Modules.Utils.Vector vec = new CounterStrikeSharp.API.Modules.Utils.Vector(receiver.Pawn.Value.AbsOrigin.X, receiver.Pawn.Value.AbsOrigin.Y, receiver.Pawn.Value.AbsOrigin.Z + 20);
				CEnvEntityMaker Maker = Utilities.CreateEntityByName<CEnvEntityMaker>("env_entity_maker");
				Maker.Template = entPT.Entity.Name;
				Maker.Flags = 0;
				Maker.DispatchSpawn();
				Maker.Teleport(vec);
				Maker.AcceptInput("ForceSpawn");
				Maker.Remove();
				new CounterStrikeSharp.API.Modules.Timers.Timer(0.2f, () =>
				{
					if (receiver != null && receiver.IsValid && EW.CheckDictionary(receiver, EW.g_BannedPlayer)) EW.g_BannedPlayer[receiver].bFixSpawnItem = false;
				});
				UI.EWChatAdmin("Reply.Spawn.Notify", $"{UI.PlayerInfo(admin)}{EW.g_Scheme.color_warning}", $"{Item.Color}{Item.Name}({Item.ShortName}){EW.g_Scheme.color_warning}", $"{UI.PlayerInfo(receiver)}");
			} else
			{
				UI.EWReplyInfo(admin, "Reply.Spawn.NoSpawner", bConsole);
				return;
			}
		}
	}
}
