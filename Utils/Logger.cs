using POGOProtos.Map.Pokemon;
using PokemonGo.RocketAPI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static POGOProtos.Networking.Responses.CatchPokemonResponse.Types;

namespace PidgeyBot.Utils
{


    /// <summary>
    /// The ConsoleLogger is a simple logger which writes all logs to the Console.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Log a specific message by LogLevel. Won't log if the LogLevel is greater than the maxLogLevel set.
        /// </summary>
        /// <param name="message">The message to log. The current time will be prepended.</param>
        /// <param name="level">Optional. Default <see cref="LogLevel.Info"/>.</param>
        public static void Write(string message, LogLevel level = LogLevel.Info, string username = "", AuthType auth = AuthType.Google)
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;

            if (username != "")
            {
                if (auth == AuthType.Google)
                {
                    System.Console.Write($"[{ DateTime.Now.ToString("HH:mm:ss")}]");
                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.Write($"[{ username }] ");
                }
                else {
                    System.Console.Write($"[{ DateTime.Now.ToString("HH:mm:ss")}]");
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.Write($"[{ username }] ");
                }

            }
            else
                System.Console.Write($"[{ DateTime.Now.ToString("HH:mm:ss")}] ");

            switch (level)
            {
                case LogLevel.Error:
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Warning:
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Info:
                    System.Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Debug:
                    System.Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                default:
                    break;
            }
            System.Console.Write($"{ message }\n");
        }

        public static void WriteCatch(MapPokemon pokemon = null, dynamic encounter = null, CatchStatus status = CatchStatus.CatchError, string username = "", AuthType auth = AuthType.Google)
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;

            if (username != "")
            {
                if (auth == AuthType.Google)
                {
                    System.Console.Write($"[{ DateTime.Now.ToString("HH:mm:ss")}]");
                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.Write($"[{ username }] ");
                }
                else
                {
                    System.Console.Write($"[{ DateTime.Now.ToString("HH:mm:ss")}]");
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.Write($"[{ username }] ");
                }

            }
            else
                System.Console.Write($"[{ DateTime.Now.ToString("HH:mm:ss")}] ");

            System.Console.ForegroundColor = ConsoleColor.White;
            if (pokemon != null || encounter != null)
            {
                    switch (status)
                    {
                        case CatchStatus.CatchSuccess:
                            System.Console.Write("Caught a ");
                            System.Console.ForegroundColor = ConsoleColor.Red;
                            try
                            {
                                System.Console.Write($"{ pokemon.PokemonId }");
                            } catch (Exception)
                            {
                                System.Console.Write($"{encounter?.PokemonData.PokemonId} (yay)");
                            }
                            System.Console.ForegroundColor = ConsoleColor.White;
                            System.Console.Write($" ({encounter?.WildPokemon?.PokemonData?.Cp} CP)\n");
                            break;
                        case CatchStatus.CatchEscape:
                        case CatchStatus.CatchFlee:
                            System.Console.Write("We missed a ");
                            System.Console.ForegroundColor = ConsoleColor.Red;
                            try
                            {
                                System.Console.Write($"{ pokemon.PokemonId}");
                            }
                            catch (Exception)
                            {
                                System.Console.Write($"{encounter?.PokemonData.PokemonId} (yay)");
                            }
                            System.Console.ForegroundColor = ConsoleColor.White;
                            System.Console.Write($" ({encounter?.WildPokemon?.PokemonData?.Cp} CP)\n");
                            break;
                        case CatchStatus.CatchError:
                            System.Console.Write("Catch Error by ");
                            System.Console.ForegroundColor = ConsoleColor.Red;
                            try
                            {
                                System.Console.Write($"{ pokemon.PokemonId}");
                            }
                            catch (Exception)
                            {
                                System.Console.Write($"{encounter?.PokemonData.PokemonId} (yay)");
                            }
                            System.Console.ForegroundColor = ConsoleColor.White;    
                            System.Console.Write($" ({encounter?.WildPokemon?.PokemonData?.Cp} CP)\n");
                            break;
                    }
            }
        }

        public enum LogLevel
        {
            None = 0,
            Error = 1,
            Warning = 2,
            Info = 3,
            Debug = 4
        }
    }
}
