using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PidgeyBot;
using PidgeyBot.Utils;

namespace PoGo.NecroBot.Logic.Tasks
{
    public class RenamePokemonTask
    {
        public static async Task Execute(PidgeyInstance pidgey)
        {
            var pokemons = await pidgey._inventory.GetPokemons();

            foreach (var pokemon in pokemons)
            {
                var perfection = Math.Round(PokemonInfo.CalculatePokemonPerfection(pokemon));
                var pokemonName = pokemon.PokemonId.ToString();
                if (pokemonName.Length > 10 - perfection.ToString().Length)
                {
                    pokemonName = pokemonName.Substring(0, 10 - perfection.ToString().Length);
                }
                var newNickname = $"{pokemonName}_{perfection}";

                if (/*perfection > pidgey._client.Settings.KeepMinIVPercentage &&*/ newNickname != pokemon.Nickname && pidgey._client.Settings.RenamePokemons)
                {
                    var result = await pidgey._client.Inventory.NicknamePokemon(pokemon.Id, newNickname);

                    Logger.Write($"Pokemon {pokemon.PokemonId} ({pokemon.PokemonId}) renamed from {pokemon.Nickname} to {newNickname}.", Logger.LogLevel.Info, pidgey._trainerName, pidgey._authType);
                }
                else if (newNickname == pokemon.Nickname && !pidgey._client.Settings.RenamePokemons)
                {
                    var result = await pidgey._client.Inventory.NicknamePokemon(pokemon.Id, pokemon.PokemonId.ToString());

                    Logger.Write($"Pokemon {pokemon.PokemonId} ({pokemon.PokemonId}) renamed from {pokemon.Nickname} to {newNickname}.", Logger.LogLevel.Info, pidgey._trainerName, pidgey._authType);
                }
            }

        }
    }
}
