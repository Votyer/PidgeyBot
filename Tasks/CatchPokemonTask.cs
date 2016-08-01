#region using directives

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI;
using PidgeyBot.Utils;
using PidgeyBot.Logic;
using PidgeyBot;
using static PidgeyBot.Utils.Logger;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public static class CatchPokemonTask
    {
        public static async Task Execute(PidgeyInstance pidgey, dynamic encounter, MapPokemon pokemon,
            FortData currentFortData = null, ulong encounterId = 0)
        {
            CatchPokemonResponse caughtPokemonResponse;
            do
            {
                float probability = encounter?.CaptureProbability?.CaptureProbability_[0];

                var pokeball = await GetBestBall(pidgey, encounter, probability);
                if (pokeball == ItemId.ItemUnknown)
                {
                    Logger.Write($"No Pokeballs - We missed a {pokemon.PokemonId} with CP {encounter?.WildPokemon?.PokemonData?.Cp}", LogLevel.Info, pidgey._trainerName, pidgey._authType);
                    return;
                }

                var isLowProbability = probability < 0.35;
                var isHighCp = encounter != null &&
                               (encounter is EncounterResponse
                                   ? encounter.WildPokemon?.PokemonData?.Cp
                                   : encounter.PokemonData?.Cp) > 400;
                var isHighPerfection =
                    PokemonInfo.CalculatePokemonPerfection(encounter is EncounterResponse
                        ? encounter?.WildPokemon?.PokemonData
                        : encounter?.PokemonData) >= pidgey._client.Settings.KeepMinIVPercentage;

                if ((isLowProbability && isHighCp) || isHighPerfection)
                {
                    await UseBerry(pidgey, encounter is EncounterResponse ? pokemon.EncounterId : encounterId,
                        encounter is EncounterResponse ? pokemon.SpawnPointId : currentFortData?.Id);
                }

                var distance = LocationUtils.CalculateDistanceInMeters(pidgey._client.CurrentLatitude,
                    pidgey._client.CurrentLongitude,
                    encounter is EncounterResponse ? pokemon.Latitude : currentFortData.Latitude,
                    encounter is EncounterResponse ? pokemon.Longitude : currentFortData.Longitude);

                caughtPokemonResponse =
                    await pidgey._client.Encounter.CatchPokemon(
                        encounter is EncounterResponse ? pokemon.EncounterId : encounterId,
                        encounter is EncounterResponse ? pokemon.SpawnPointId : currentFortData.Id, pokeball);

                //var evt = new PokemonCaptureEvent {Status = caughtPokemonResponse.Status};

                if (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
                {
                    try
                    {
                        WriteCatch(pokemon, encounter, caughtPokemonResponse.Status, pidgey._trainerName, pidgey._authType);
                        var profile = await pidgey._client.Player.GetPlayer();
                        foreach (var xp in caughtPokemonResponse.CaptureAward.Xp)
                        {
                            pidgey._stats.TotalExperience += xp;
                        }
                        pidgey._stats.TotalPokemons++;
                        pidgey._stats.TotalStardust = profile.PlayerData.Currencies.ToArray()[1].Amount;
                    }
                    catch (Exception e)
                    {
                    }
                }
                
                await pidgey._inventory.RefreshCachedInventory();
                await Task.Delay(2000);
            } while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed ||
                     caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);
        }

        private static async Task<ItemId> GetBestBall(PidgeyInstance pidgey, dynamic encounter, float probability)
        {
            var pokemonCp = encounter is EncounterResponse
                ? encounter?.WildPokemon?.PokemonData?.Cp
                : encounter?.PokemonData?.Cp;
            var iV =
                Math.Round(
                    PokemonInfo.CalculatePokemonPerfection(encounter is EncounterResponse
                        ? encounter?.WildPokemon?.PokemonData
                        : encounter?.PokemonData));

            var pokeBallsCount = await pidgey._inventory.GetItemAmountByType(ItemId.ItemPokeBall);
            var greatBallsCount = await pidgey._inventory.GetItemAmountByType(ItemId.ItemGreatBall);
            var ultraBallsCount = await pidgey._inventory.GetItemAmountByType(ItemId.ItemUltraBall);
            var masterBallsCount = await pidgey._inventory.GetItemAmountByType(ItemId.ItemMasterBall);

            if (masterBallsCount > 0 && pokemonCp >= 1200)
                return ItemId.ItemMasterBall;
            if (ultraBallsCount > 0 && pokemonCp >= 1000)
                return ItemId.ItemUltraBall;
            if (greatBallsCount > 0 && pokemonCp >= 750)
                return ItemId.ItemGreatBall;

            if (ultraBallsCount > 0 && iV >= pidgey._client.Settings.KeepMinIVPercentage && probability < 0.40)
                return ItemId.ItemUltraBall;

            if (greatBallsCount > 0 && iV >= pidgey._client.Settings.KeepMinIVPercentage && probability < 0.50)
                return ItemId.ItemGreatBall;

            if (greatBallsCount > 0 && pokemonCp >= 300)
                return ItemId.ItemGreatBall;

            if (pokeBallsCount > 0)
                return ItemId.ItemPokeBall;
            if (greatBallsCount > 0)
                return ItemId.ItemGreatBall;
            if (ultraBallsCount > 0)
                return ItemId.ItemUltraBall;
            if (masterBallsCount > 0)
                return ItemId.ItemMasterBall;

            return ItemId.ItemUnknown;
        }

        private static async Task UseBerry(PidgeyInstance pidgey, ulong encounterId, string spawnPointId)
        {
            var inventoryBalls = await pidgey._inventory.GetItems();
            var berries = inventoryBalls.Where(p => p.ItemId == ItemId.ItemRazzBerry);
            var berry = berries.FirstOrDefault();

            if (berry == null || berry.Count <= 0)
                return;

            await pidgey._client.Encounter.UseCaptureItem(encounterId, ItemId.ItemRazzBerry, spawnPointId);
            berry.Count -= 1;
            Logger.Write("Use Berry, " + berry.Count + " remaining");

            await Task.Delay(1500);
        }
    }
}