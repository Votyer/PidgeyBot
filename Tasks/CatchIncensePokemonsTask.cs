using System.Threading;
using System.Threading.Tasks;
using POGOProtos.Enums;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI;
using PidgeyBot.Utils;
using static PidgeyBot.Utils.Logger;
using PidgeyBot.Logic;
using PidgeyBot;

namespace PoGo.NecroBot.Logic.Tasks
{
    public static class CatchIncensePokemonsTask
    {
        public static async Task Execute(PidgeyInstance pidgey)
        {
            var incensePokemon = await pidgey._client.Map.GetIncensePokemons();
            if (incensePokemon.Result == GetIncensePokemonResponse.Types.Result.IncenseEncounterAvailable)
            {
                var pokemon = new MapPokemon
                {
                    EncounterId = incensePokemon.EncounterId,
                    ExpirationTimestampMs = incensePokemon.DisappearTimestampMs,
                    Latitude = incensePokemon.Latitude,
                    Longitude = incensePokemon.Longitude,
                    PokemonId = (PokemonId)incensePokemon.PokemonTypeId,
                    SpawnPointId = incensePokemon.EncounterLocation
                };


                var distance = LocationUtils.CalculateDistanceInMeters(pidgey._client.CurrentLatitude,
                    pidgey._client.CurrentLongitude, pokemon.Latitude, pokemon.Longitude);
                //await Task.Delay(distance > 100 ? 15000 : 500);

                var encounter =
                    await pidgey._client.Encounter.EncounterIncensePokemon((long)pokemon.EncounterId, pokemon.SpawnPointId);

                if (encounter.Result == IncenseEncounterResponse.Types.Result.IncenseEncounterSuccess)
                {
                    await CatchPokemonTask.Execute(pidgey, encounter, pokemon);
                }
                else if (encounter.Result == IncenseEncounterResponse.Types.Result.PokemonInventoryFull)
                {
                    if (pidgey._client.Settings.AutoTransfer)
                    {
                        Logger.Write($"PokemonInventory is Full. Transferring pokemons...", LogLevel.Warning);
                        await TransferDuplicatePokemonTask.Execute(pidgey);
                    }
                    else
                        Logger.Write($"PokemonInventory is Full. Please Transfer pokemon manually or set TransferDuplicatePokemon to true in settings...", LogLevel.Warning);

                }
                else
                {
                    Logger.Write($"Encounter problem: {encounter.Result}", LogLevel.Error);
                }

            }
        }
    }
}