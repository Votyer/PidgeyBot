#region using directives

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PokemonGo.RocketAPI;
using POGOProtos.Inventory.Item;
using PidgeyBot.Logic;
using PidgeyBot.Utils;
using PidgeyBot;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class EvolvePokemonTask
    {
        private static DateTime _lastLuckyEggTime;
        public static async Task Execute(PidgeyInstance pidgey)
        {
            if (pidgey._client.Settings.UseLuckyEggs)
            {
                await UseLuckyEgg(pidgey);
            }

            var pokemonToEvolveTask = await pidgey._inventory.GetPokemonToEvolve(pidgey._client.Settings.PokemonsToEvolve);

            var pokemonToEvolve = pokemonToEvolveTask;
            foreach (var pokemon in pokemonToEvolve)
            {
                var evolveResponse = await pidgey._client.Inventory.EvolvePokemon(pokemon.Id);

                switch (evolveResponse.Result)
                {
                    case POGOProtos.Networking.Responses.EvolvePokemonResponse.Types.Result.Success:
                        Logger.Write("Evolved a " + pokemon.PokemonId + " (+" + evolveResponse.ExperienceAwarded + "XP)", Logger.LogLevel.Info, pidgey._trainerName, pidgey._authType);
                        break;
                }

                pidgey._stats.TotalExperience += evolveResponse.ExperienceAwarded;
            }
        }

        public static async Task UseLuckyEgg(PidgeyInstance pidgey)
        {
            var inventoryContent = await pidgey._inventory.GetItems();

            var luckyEggs = inventoryContent.Where(p => p.ItemId == ItemId.ItemLuckyEgg);
            var luckyEgg = luckyEggs.FirstOrDefault();

            if (luckyEgg == null || luckyEgg.Count <= 0 || _lastLuckyEggTime.AddMinutes(30).Ticks > DateTime.Now.Ticks)
                return;

            _lastLuckyEggTime = DateTime.Now;
            await pidgey._client.Inventory.UseItemXpBoost();
            var refreshCachedInventory = await pidgey._inventory.RefreshCachedInventory();
            Logger.Write("Use Lucky Egg, " + luckyEgg.Count + " remaining", Logger.LogLevel.Info, pidgey._trainerName, pidgey._authType);
            await Task.Delay(2000);
        }
    }
}