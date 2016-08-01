#region using directives

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using PidgeyBot.Utils;
using static PidgeyBot.Utils.Logger;
using PidgeyBot.Logic;
using PokemonGo.RocketAPI;
using PidgeyBot;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public static class CatchNearbyPokemonsTask
    {
        public static async Task Execute(PidgeyInstance pidgey)
        {
            var pokemons = await GetNearbyPokemons(pidgey._client);
            foreach (var pokemon in pokemons)
            {
                var distance = LocationUtils.CalculateDistanceInMeters(pidgey._client.CurrentLatitude,
                    pidgey._client.CurrentLongitude, pokemon.Latitude, pokemon.Longitude);
                await Task.Delay(distance > 100 ? 15000 : 500);

                var encounter = await pidgey._client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId);

                if (encounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                {
                    await CatchPokemonTask.Execute(pidgey, encounter, pokemon);
                }
                else if (encounter.Status == EncounterResponse.Types.Status.PokemonInventoryFull)
                {
                    if (pidgey._client.Settings.AutoTransfer)
                    {
                        Logger.Write($"PokemonInventory is Full. Transferring pokemons...", LogLevel.Info);
                        await TransferDuplicatePokemonTask.Execute(pidgey);
                    }
                    else
                        Logger.Write($"PokemonInventory is Full. Please Transfer pokemon manually or set TransferDuplicatePokemon to true in settings...", LogLevel.Warning);

                }
                else
                {
                    Logger.Write($"Encounter problem: {encounter.Status}", LogLevel.Error);
                }

                // If pokemon is not last pokemon in list, create delay between catches, else keep moving.
                if (!Equals(pokemons.ElementAtOrDefault(pokemons.Count() - 1), pokemon))
                {
                    await Task.Delay(pidgey._client.Settings.DelayBetweenPokemonCatch);
                }
            }
        }

        private static async Task<IOrderedEnumerable<MapPokemon>> GetNearbyPokemons(Client _client)
        {
            var mapObjects = await _client.Map.GetMapObjects();

            var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons)
                .OrderBy(
                    i =>
                        LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude,
                            i.Latitude, i.Longitude));

            return pokemons;
        }
    }
}