using System.Threading.Tasks;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using PidgeyBot.Logic;
using PokemonGo.RocketAPI;
using static PidgeyBot.Utils.Logger;
using PidgeyBot.Utils;
using PidgeyBot;

namespace PoGo.NecroBot.Logic.Tasks
{
    public static class CatchLurePokemonsTask
    {
        public static async Task Execute(PidgeyInstance pidgey, FortData currentFortData)
        {
            var fortId = currentFortData.Id;

            var pokemonId = currentFortData.LureInfo.ActivePokemonId;

            var encounterId = currentFortData.LureInfo.EncounterId;
            var encounter = await pidgey._client.Encounter.EncounterLurePokemon(encounterId, fortId);

            if (encounter.Result == DiskEncounterResponse.Types.Result.Success)
            {
                await CatchPokemonTask.Execute(pidgey, encounter, null, currentFortData, encounterId);
            }
            else if (encounter.Result == DiskEncounterResponse.Types.Result.PokemonInventoryFull)
            {
                if (pidgey._clientSettings.AutoTransfer)
                {
                    Logger.Write($"PokemonInventory is Full. Transferring pokemons...", LogLevel.Info, pidgey._trainerName, pidgey._authType);
                    await TransferDuplicatePokemonTask.Execute(pidgey);
                }
                else
                    Logger.Write($"PokemonInventory is Full. Please Transfer pokemon manually or set TransferDuplicatePokemon to true in settings...", LogLevel.Warning, pidgey._trainerName, pidgey._authType);

            }
            else
            {
                Logger.Write($"Encounter problem: Lure Pokemon {encounter.Result}", LogLevel.Error);
            }

        }
    }
}