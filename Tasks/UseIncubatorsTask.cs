using System.Linq;
using System.Threading.Tasks;
using POGOProtos.Inventory.Item;
using PidgeyBot;
using PidgeyBot.Utils;

namespace PoGo.NecroBot.Logic.Tasks
{
    class UseIncubatorsTask
    {
        public static async Task Execute(PidgeyInstance pidgey)
        {
            // Refresh inventory so that the player stats are fresh
            await pidgey._inventory.RefreshCachedInventory();

            var playerStats = (await pidgey._inventory.GetPlayerStats()).FirstOrDefault();
            if (playerStats == null)
                return;

            var kmWalked = playerStats.KmWalked;

            var incubators = (await pidgey._inventory.GetEggIncubators())
                .Where(x => x.UsesRemaining > 0 || x.ItemId == ItemId.ItemIncubatorBasicUnlimited)
                .OrderByDescending(x => x.ItemId == ItemId.ItemIncubatorBasicUnlimited)
                .ToList();

            var unusedEggs = (await pidgey._inventory.GetEggs())
                .Where(x => string.IsNullOrEmpty(x.EggIncubatorId))
                .OrderBy(x => x.EggKmWalkedTarget - x.EggKmWalkedStart)
                .ToList();

            foreach (var incubator in incubators)
            {
                if (incubator.PokemonId == 0)
                {
                    // Unlimited incubators prefer short eggs, limited incubators prefer long eggs
                    var egg = incubator.ItemId == ItemId.ItemIncubatorBasicUnlimited
                        ? unusedEggs.FirstOrDefault()
                        : unusedEggs.LastOrDefault();

                    if (egg == null)
                        continue;

                    var response = await pidgey._client.Inventory.UseItemEggIncubator(incubator.Id, egg.Id);
                    unusedEggs.Remove(egg);

                    Logger.Write("We used an Incubator. On a " + egg.EggKmWalkedTarget + " km Egg", Logger.LogLevel.Info, pidgey._trainerName, pidgey._authType);

                    await Task.Delay(500);
                }
            }
        }
    }
}