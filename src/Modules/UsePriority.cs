using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using EntWatchSharp.Items;

namespace EntWatchSharp.Modules
{
    internal class UsePriority
    {
        public bool Activate;

        bool LockSpam;
        bool OneButton;
        Item OneItem;
        public UsePriority()
        {
            Activate = true;
            LockSpam = false;
            OneButton = false;
            OneItem = null;
        }
        public void DetectUse(CCSPlayerController UPlayer)
        {
            if (!EW.g_EWPlayer[UPlayer].UsePriorityPlayer.Activate || LockSpam || !OneButton) return;
            if ((UPlayer.Buttons & PlayerButtons.Use) != 0)
            {
                LockSpam = true;
                var Timer = new CounterStrikeSharp.API.Modules.Timers.Timer(0.5f, UsePriorityTimer);
                
                int iNum = 0;
				foreach(Ability AbilityTest in OneItem.AbilityList.ToList())
                {
                    if(AbilityTest.Ignore)
                    {
                        iNum++;
                        continue;
                    }
                    break;
                }
                if (iNum + 1 > OneItem.AbilityList.Count) return; //All Ignore

				if (OneItem.CheckDelay() && OneItem.AbilityList[iNum].Mode != 1 && OneItem.AbilityList[iNum].Mode < 6 && OneItem.AbilityList[iNum].fLastUse < EW.fGameTime && OneItem.AbilityList[iNum].Entity.IsValid && !OneItem.AbilityList[iNum].LockItem)
                {
                    //OneItem.AbilityList[0].Entity.AcceptInput("Press", UPlayer.PlayerPawn.Value, UPlayer.PlayerPawn.Value);
                    OneItem.AbilityList[iNum].Entity.AcceptInput("Use", UPlayer.PlayerPawn.Value, UPlayer.PlayerPawn.Value);
                    //Console.WriteLine($"Player: {UPlayer.PlayerName} pressed E ButtonID: {OneItem.AbilityList[0].Entity.Index}");
                }
            }
        }
        private void UsePriorityTimer()
        {
            LockSpam = false;
        }
        public void UpdateCountButton(CCSPlayerController UPlayer)
        {
            OneButton = false;
            int iCount = 0;
            foreach (Item ItemTest in EW.g_ItemList.ToList())
            {
                if (ItemTest.Owner == UPlayer)
                {
                    int iCountWithoutIgnore = 0;
                    foreach (Ability AbilityTest in ItemTest.AbilityList.ToList())
                        if (!AbilityTest.Ignore && AbilityTest.Mode != 8) iCountWithoutIgnore++;
					iCount += iCountWithoutIgnore;
                    if (!ItemTest.UsePriority || iCount > 1)
                    {
                        OneButton = false;
                        OneItem = null;
                        break;
                    }
                    if (iCount == 1)
                    {
                        OneButton = true;
                        OneItem = ItemTest;
                    }
                }
            }
        }
    }
}
