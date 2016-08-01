using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Map.Fort;
using PidgeyBot;
using PidgeyBot.Utils;
using POGOProtos.Inventory.Item;

namespace PoGo.NecroBot.Logic.Tasks
{
    class UseNearbyPokestopsTask
    {
        //Please do not change GetPokeStops() in this file, it's specifically set
        //to only find stops within 40 meters
        //this is for gpx pathing, we are not going to the pokestops,
        //so do not make it more than 40 because it will never get close to those stops.
        public static async Task Execute(PidgeyInstance pidgey)
        {
            var pokestopList = await GetPokeStops(pidgey);

            while (pokestopList.Any())
            {
                pokestopList =
                    pokestopList.OrderBy(
                        i =>
                            LocationUtils.CalculateDistanceInMeters(pidgey._client.CurrentLatitude,
                                pidgey._client.CurrentLongitude, i.Latitude, i.Longitude)).ToList();
                var pokeStop = pokestopList[0];
                pokestopList.RemoveAt(0);

                await pidgey._client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                var fortSearch =
                    await pidgey._client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                if (fortSearch.ExperienceAwarded > 0)
                {
                    Logger.Write("Farmed XP: " + fortSearch.ExperienceAwarded + " Gems: " + fortSearch.GemsAwarded + " Items: " + GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded));
                }

                await Task.Delay(1000);

                await RecycleItemsTask.Execute(pidgey);

                if (pidgey._client.Settings.AutoTransfer)
                {
                    await TransferDuplicatePokemonTask.Execute(pidgey);
                }
            }
        }

        public static string GetSummedFriendlyNameOfItemAwardList(IEnumerable<ItemAward> items)
        {
            var enumerable = items as IList<ItemAward> ?? items.ToList();

            if (!enumerable.Any())
                return string.Empty;

            return
                enumerable.GroupBy(i => i.ItemId)
                    .Select(kvp => new { ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount) })
                    .Select(y => $"{y.Amount} x {y.ItemName}")
                    .Aggregate((a, b) => $"{a}, {b}");
        }

        private static async Task<List<FortData>> GetPokeStops(PidgeyInstance pidgey)
        {
            var mapObjects = await pidgey._client.Map.GetMapObjects();

            // Wasn't sure how to make this pretty. Edit as needed.
            var pokeStops = mapObjects.MapCells.SelectMany(i => i.Forts)
                .Where(
                    i =>
                        i.Type == FortType.Checkpoint &&
                        i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime() &&
                        ( // Make sure PokeStop is within 40 meters or else it is pointless to hit it
                            LocationUtils.CalculateDistanceInMeters(
                                pidgey._client.Settings.DefaultLatitude, pidgey._client.Settings.DefaultLongitude,
                                i.Latitude, i.Longitude) < 40) ||
                        pidgey._client.Settings.MaxTravelDistanceInMeters == 0
                );

            return pokeStops.ToList();
        }
    }


}
