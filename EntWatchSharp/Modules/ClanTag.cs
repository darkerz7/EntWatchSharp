using CounterStrikeSharp.API;
using EntWatchSharp.Items;
using CounterStrikeSharp.API.Core;

namespace EntWatchSharp.Modules
{
	static class ClanTag
	{
		public static void UpdateClanTag()
		{
			if (Cvar.ClanTag && Cvar.ClanTagInfo)
			{
				foreach (Item ItemTest in EW.g_ItemList.ToList())
				{
					if (ItemTest.Owner != null) ConstructClanTag(ItemTest);
				}
				/*Utilities.GetPlayers().ForEach(player =>
				{
					if (player.IsValid)
					{
						EventNextlevelChanged fakeEvent = new(false);
						fakeEvent.FireEventToClient(player);
					}
				});*/
			}
		}

		public static void UpdatePickUp(Item ItemTest)
		{
			if (Cvar.ClanTag)
			{
				ConstructClanTag(ItemTest);
				/*Utilities.GetPlayers().ForEach(player =>
				{
					if (player.IsValid)
					{
						EventNextlevelChanged fakeEvent = new(false);
						fakeEvent.FireEventToClient(player);
					}
				});*/
			}
		}

		public static void RemoveClanTag(CCSPlayerController player)
		{
			SetClanTag(player, "");
		}

		private static void ConstructClanTag(Item ItemTest)
		{
			string sClanTag = $"{ItemTest.ShortName}";
			if (Cvar.ClanTagInfo)
			{
				sClanTag += " ";
				if (ItemTest.CheckDelay())
				{
					int iAbilityCount = 0;
					foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
					{
						if (++iAbilityCount > Cvar.DisplayAbility) break;
						if (!AbilityTest.Ignore) sClanTag += $"[{AbilityTest.GetMessage()}]";
					}

				}
				else sClanTag += $"[-{Math.Round(ItemTest.fDelay - EW.fGameTime, 1)}]";
			}
			SetClanTag(ItemTest.Owner, sClanTag);
		}

		private static void SetClanTag(CCSPlayerController player, string sClanTag)
		{
			if (sClanTag.Length > 24) player.Clan = sClanTag[..23];
			else player.Clan = sClanTag;
			Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");

			EventNextlevelChanged fakeEvent = new(false);
			fakeEvent.FireEventToClient(player);    // <- Need Tests with real people
		}
	}
}
