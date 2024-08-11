using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using EntWatchSharp.Items;

namespace EntWatchSharp.Modules
{
    internal class UsePriority
    {
        public bool Activate;

        CCSPlayerController UPlayer;
        bool LockSpam;
        bool OneButton;
        Item OneItem;
        public UsePriority(CCSPlayerController player)
        {
            Activate = true;
            UPlayer = player;
            LockSpam = false;
            OneButton = false;
            OneItem = null;
        }
        public void DetectUse()
        {
            if (!EW.g_UsePriorityPlayer[UPlayer].Activate || LockSpam || !OneButton) return;
            if ((UPlayer.Buttons & PlayerButtons.Use) != 0)
            {
                LockSpam = true;
                var Timer = new CounterStrikeSharp.API.Modules.Timers.Timer(0.5f, UsePriorityTimer);
                if (OneItem.CheckDelay() && OneItem.AbilityList[0].Mode != 1 && OneItem.AbilityList[0].Mode != 6 && OneItem.AbilityList[0].Mode != 7 && OneItem.AbilityList[0].fLastUse < EW.fGameTime && OneItem.AbilityList[0].Entity.IsValid)
                {
                    //OneItem.AbilityList[0].Entity.AcceptInput("Press", UPlayer.PlayerPawn.Value, UPlayer.PlayerPawn.Value);
                    OneItem.AbilityList[0].Entity.AcceptInput("Use", UPlayer.PlayerPawn.Value, UPlayer.PlayerPawn.Value);
                    //Console.WriteLine($"Player: {UPlayer.PlayerName} pressed E ButtonID: {OneItem.AbilityList[0].Entity.Index}");
                }
            }
        }
        private void UsePriorityTimer()
        {
            LockSpam = false;
        }
        public void UpdateCountButton()
        {
            OneButton = false;
            int iCount = 0;
            foreach (Item ItemTest in EW.g_ItemList.ToList())
            {
                if (ItemTest.Owner == UPlayer)
                {
                    iCount += ItemTest.AbilityList.Count;
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
