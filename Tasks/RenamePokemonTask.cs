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
                string newNickname = String.Format(RenameTemplate, pokemonName, GetPerfectName(perfection));
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
        private static string GetPerfectName(double perfectScore)
        {
            string result = string.Empty;
            var TenDigit = Convert.ToInt32(Math.Floor(perfectScore / 10).ToString());
            var SingleDigit = perfectScore % 10;

            switch (TenDigit)
            {
                case 10:
                    result = string.Format("S{0}", SingleDigit);
                    break;
                case 9:
                    result = string.Format("A{0}", SingleDigit);
                    break;
                case 8:
                    result = string.Format("B{0}", SingleDigit);
                    break;
                case 7:
                    result = string.Format("C{0}", SingleDigit);
                    break;
                case 6:
                    result = string.Format("D{0}", SingleDigit);
                    break;
                case 5:
                    result = string.Format("E{0}", SingleDigit);
                    break;
                case 4:
                    result = string.Format("F{0}", SingleDigit);
                    break;
                case 3:
                    result = string.Format("G{0}", SingleDigit);
                    break;
                case 2:
                    result = string.Format("H{0}", SingleDigit);
                    break;
                case 1:
                    result = string.Format("I{0}", SingleDigit);
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}