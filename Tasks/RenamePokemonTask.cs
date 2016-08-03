#region using directives

using PidgeyBot;
using PidgeyBot.Utils;
using System;
using System.Globalization;
using System.Threading.Tasks;

#endregion

namespace PoGo.NecroBot.Logic.Tasks
{
    public class RenamePokemonTask
    {
        public static async Task Execute(PidgeyInstance pidgey)
        {
            await pidgey._inventory.RefreshCachedInventory();
            var pokemons = await pidgey._inventory.GetPokemons();

            foreach (var pokemon in pokemons)
            {
                double perfection = Math.Round(PokemonInfo.CalculatePokemonPerfection(pokemon));
                string pokemonName = pokemon.PokemonId.ToString();

                switch (pokemonName.ToLower())
                {
                    case "nidoranfemale":
                        pokemonName = "Nidoran♀";
                        break;
                    case "nidoranmale":
                        pokemonName = "Nidoran♂";
                        break; 
                    default:
                        break;
                }

                string RenameTemplate = "{0}_{1}";
                // iv number + templating part + pokemonName <= 12
                int nameLength = 12 - (perfection.ToString(CultureInfo.InvariantCulture).Length + RenameTemplate.Length - 6);
                if (pokemonName.Length > nameLength)
                {
                    pokemonName = pokemonName.Substring(0, nameLength);
                }
                string newNickname = String.Format(RenameTemplate, pokemonName, perfection);
                string oldNickname = (pokemon.Nickname.Length != 0) ? pokemon.Nickname : pokemon.PokemonId.ToString();

                if (perfection >= pidgey._clientSettings.KeepMinIVPercentage && newNickname != oldNickname &&
                    pidgey._clientSettings.RenamePokemons)
                {
                    await pidgey._client.Inventory.NicknamePokemon(pokemon.Id, newNickname);

                    Logger.Write("Renamed " + oldNickname + " to " + newNickname, Logger.LogLevel.Info, pidgey._trainerName, pidgey._authType);
                }
            }

            await pidgey._inventory.RefreshCachedInventory();
        }
    }
}