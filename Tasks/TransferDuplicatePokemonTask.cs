#region using directives

using System.Linq;
using System.Threading.Tasks;
using PokemonGo.RocketAPI;
using PidgeyBot.Logic;
using PidgeyBot.Utils;
using PidgeyBot;
using System;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class TransferDuplicatePokemonTask
    {
        public static async Task Execute(PidgeyInstance pidgey)
        {
            var duplicatePokemons =
                await pidgey._inventory.GetDuplicatePokemonToTransfer(pidgey._clientSettings.KeepPokemonsThatCanEvolve, pidgey._clientSettings.PrioritizeIVOverCP,
                    pidgey._clientSettings.PokemonsNotToTransfer);

            var pokemonSettings = await pidgey._inventory.GetPokemonSettings();
            var pokemonFamilies = await pidgey._inventory.GetPokemonFamilies();

            foreach (var duplicatePokemon in duplicatePokemons)
            {
                if (duplicatePokemon.Cp >= pidgey._clientSettings.KeepMinCP ||
                    PokemonInfo.CalculatePokemonPerfection(duplicatePokemon) > pidgey._clientSettings.KeepMinIVPercentage)
                {
                    continue;
                }

                await pidgey._client.Inventory.TransferPokemon(duplicatePokemon.Id);
                await pidgey._inventory.DeletePokemonFromInvById(duplicatePokemon.Id);

                var bestPokemonOfType = pidgey._clientSettings.PrioritizeIVOverCP
                    ? await pidgey._inventory.GetHighestPokemonOfTypeByIv(duplicatePokemon)
                    : await pidgey._inventory.GetHighestPokemonOfTypeByCp(duplicatePokemon);

                if (bestPokemonOfType == null)
                    bestPokemonOfType = duplicatePokemon;
                
                pidgey._stats.TotalPokemonsTransfered++;

                Logger.Write("Transfered a " + duplicatePokemon.PokemonId + " with " + duplicatePokemon.Cp + "CP (" + Math.Round(PokemonInfo.CalculatePokemonPerfection(duplicatePokemon),2) + "%)", Logger.LogLevel.Info, pidgey._trainerName, pidgey._authType);
            }
        }
    }
}