﻿#region using directives

using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Map.Fort;
using PidgeyBot.Logic;
using PokemonGo.RocketAPI;
using PidgeyBot.Utils;
using static PidgeyBot.Utils.Logger;
using PidgeyBot;
using POGOProtos.Inventory.Item;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public static class FarmPokestopsTask
    {
        public static async Task Execute(PidgeyInstance pidgey)
        {
            var mapObjects = await pidgey._client.Map.GetMapObjects();

            Console.WriteLine(mapObjects.Item1.MapCells);

            var pokeStops = mapObjects.Item1.MapCells
                .SelectMany(i => i.Forts)
                .Where(
                    i =>
                        i.Type == FortType.Checkpoint &&
                        i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                .OrderBy(i => LocationUtils.CalculateDistanceInMeters(pidgey._client.CurrentLatitude,
                        pidgey._client.CurrentLongitude, i.Latitude, i.Longitude));

            var pokestopList = pokeStops.ToList();

            var stopsHit = 0;

            if (pokestopList.Count <= 0)
            {
                bool timeOut = false;
                do
                {
                    stopsHit++;
                    mapObjects = await pidgey._client.Map.GetMapObjects();
                    pokeStops = mapObjects.Item1.MapCells
                        .SelectMany(i => i.Forts)
                        .Where(
                            i =>
                                i.Type == FortType.Checkpoint &&
                                i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                        .OrderBy(i => LocationUtils.CalculateDistanceInMeters(pidgey._client.CurrentLatitude,
                                pidgey._client.CurrentLongitude, i.Latitude, i.Longitude));
                    pokestopList = pokeStops.ToList();
                    if(stopsHit % 5 == 0)
                        Logger.Write($"No PokeStops found. Retry {stopsHit/5}/2, please wait!", LogLevel.Info, pidgey._trainerName, pidgey._authType);
                    if (stopsHit >= 10)
                    {
                        timeOut = true;
                        Logger.Write("Timed out, we try a relog...", LogLevel.Info, pidgey._trainerName, pidgey._authType);
                    }
                    await Task.Delay(1000);
                }
                while (pokestopList.Count <= 0 && !timeOut);
            }

            stopsHit = 0;

            int i2 = 0;
            while (pokestopList.Any())
            {
                i2++;
                //resort
                pokestopList =
                    pokestopList.OrderBy(
                        i =>
                            LocationUtils.CalculateDistanceInMeters(pidgey._client.CurrentLatitude,
                                pidgey._client.CurrentLongitude, i.Latitude, i.Longitude)).ToList();
                var pokeStop = pokestopList[0];
                pokestopList.RemoveAt(0);

                var distance = LocationUtils.CalculateDistanceInMeters(pidgey._client.CurrentLatitude,
                    pidgey._client.CurrentLongitude, pokeStop.Latitude, pokeStop.Longitude);
                var fortInfo = await pidgey._client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                if (pidgey._clientSettings.UseHumanWalking)
                {
                    Logger.Write($"[{i2}/{pokeStops.Count()}] Walking to PokeStop: " + fortInfo.Name + " (" + Math.Round(distance, 2) + "m)", Logger.LogLevel.Info, pidgey._trainerName, pidgey._authType);

                    await pidgey._navigation.HumanLikeWalking(new GeoCoordinate(pokeStop.Latitude, pokeStop.Longitude),
                        pidgey._clientSettings.WalkingSpeedInKilometerPerHour,
                        async () =>
                        {
                        // Catch normal map Pokemon
                        await CatchNearbyPokemonsTask.Execute(pidgey);
                            return true;
                        });

                } else
                {
                    Logger.Write($"[{i2}/{pokeStops.Count()}] Teleport to PokeStop: " + fortInfo.Name + " (" + Math.Round(distance, 2) + "m)", Logger.LogLevel.Info, pidgey._trainerName, pidgey._authType);
                    await TeleportToPokestop(pidgey, pokeStop);
                    await Task.Delay(500);
                }

                //Catch Lure Pokemon
                if (pokeStop.LureInfo != null)
                {
                    await CatchLurePokemonsTask.Execute(pidgey, pokeStop);
                }
                
                var fortSearch = await pidgey._client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                string EggReward = fortSearch.PokemonDataEgg != null ? "1" : "0";
                Logger.Write("Farmed XP: " + fortSearch.ExperienceAwarded + " Eggs: "+ EggReward + " Gems: "+fortSearch.GemsAwarded+" Items: " + GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded), Logger.LogLevel.Info, pidgey._trainerName, pidgey._authType);

                await Task.Delay(500);

                if (++stopsHit % 5 == 0) //TODO: OR item/pokemon bag is full
                {
                    stopsHit = 0;

                    if (fortSearch.ItemsAwarded.Count > 0)
                        await pidgey._inventory.RefreshCachedInventory();

                    if (pidgey._clientSettings.RenamePokemons)
                        await RenamePokemonTask.Execute(pidgey);

                    if (pidgey._clientSettings.RecycleItems)
                        await RecycleItemsTask.Execute(pidgey);

                    if (pidgey._clientSettings.AutoEvolve || pidgey._clientSettings.EvolveAllPokemonAboveIV)
                        await EvolvePokemonTask.Execute(pidgey);

                    if (pidgey._clientSettings.AutoTransfer)
                        await TransferDuplicatePokemonTask.Execute(pidgey);

                    Logger.Write(pidgey._stats.ToStrings(pidgey._inventory), LogLevel.Info, pidgey._trainerName, pidgey._authType);
                }

            }
        }

        public static async Task TeleportToPokestop(PidgeyInstance pidgey, FortData closestPokestop)
        {
            if (closestPokestop?.Latitude == null)
                return;
            await pidgey._client.Player.UpdatePlayerLocation(closestPokestop.Latitude, closestPokestop.Longitude, pidgey._clientSettings.DefaultAltitude);
        }

        public static string GetSummedFriendlyNameOfItemAwardList(IEnumerable<ItemAward> items)
        {
            var enumerable = items as IList<ItemAward> ?? items.ToList();

            if (!enumerable.Any())
                return string.Empty;

            return
                enumerable.GroupBy(i => i.ItemId)
                    .Select(kvp => new { ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount) })
                    .Select(y => $"{y.Amount} x {y.ItemName.Substring(4)}")
                    .Aggregate((a, b) => $"{a}, {b}");
        }
    }
}