#region using directives

using System.Threading;
using System.Threading.Tasks;
using PidgeyBot;
using PidgeyBot.Utils;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class RecycleItemsTask
    {
        public static async Task Execute(PidgeyInstance pidgey)
        {
            var items = await pidgey._inventory.GetItemsToRecycle(pidgey._client.Settings);

            foreach (var item in items)
            {
                await pidgey._client.Inventory.RecycleItem(item.ItemId, item.Count);
                pidgey._stats.TotalItemsRemoved++;
                Logger.Write("Recycled Item "+ item.Count +"x " + item.ItemId, Logger.LogLevel.Info, pidgey._trainerName, pidgey._authType);
            }

            await pidgey._inventory.RefreshCachedInventory();
        }
    }
}