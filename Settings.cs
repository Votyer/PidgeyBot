using Ini;
using PidgeyBot.Utils;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PidgeyBot.Utils.Logger;

namespace PidgeyBot
{

    public class Settings : ISettings
    {
        public double DefaultLatitude { get; set; }
        public double DefaultLongitude { get; set; }
        public double DefaultAltitude { get; set; }
        public float KeepMinIVPercentage { get; set; }
        public int KeepMinCP { get; set; }
        public int KeepMinDuplicatePokemon { get; set; }
        public int MaxTravelDistanceInMeters { get; set; }

        public double WalkingSpeedInKilometerPerHour { get; set; }
        public bool AutoEvolve { get; set; }
        public bool PrioritizeIVOverCP { get; set; }
        public bool AutoTransfer { get; set; }
        public bool RecycleItems { get; set; }
        public bool UseGoogleAccounts { get; set; }
        public bool UsePtcAccounts { get; set; }
        public bool UseLuckyEggs { get; set; }
        public int AfterCatchInSeconds { get; set; }
        public float EvolveAboveIVValue { get; set; }
        public bool EvolveAllPokemonAboveIV { get; set; }
        
        //WHY
        public AuthType AuthType { get; set; }
        public string GoogleRefreshToken { get; set; }
        public string PtcUsername { get; set; }
        public string PtcPassword { get; set; }


        public bool KeepPokemonsThatCanEvolve { get; set; }

        public bool UseEggIncubators { get; set; }
        public bool RenamePokemons { get; set; }

        public int DelayBetweenPokemonCatch { get; set; }

        private ICollection<KeyValuePair<ItemId, int>> itemList;
        private ICollection<PokemonId> _pokemonsNotToTransfer;
        private ICollection<PokemonId> _pokemonsToEvolve;

        public Settings()
        {
            try {
                Write("Loading settings.ini");

                IniFile iniFile = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "settings.ini");

                DefaultLatitude = double.Parse(iniFile.IniReadValue("Location", "DefaultLatitude"), CultureInfo.InvariantCulture);
                DefaultLongitude = double.Parse(iniFile.IniReadValue("Location", "DefaultLongitude"), CultureInfo.InvariantCulture);
                DefaultAltitude = double.Parse(iniFile.IniReadValue("Location", "DefaultAltitude"), CultureInfo.InvariantCulture);
                WalkingSpeedInKilometerPerHour = double.Parse(iniFile.IniReadValue("Location", "WalkingSpeedInKilometerPerHour"), CultureInfo.InvariantCulture);
                MaxTravelDistanceInMeters = Convert.ToInt32(iniFile.IniReadValue("Location", "MaxTravelDistanceInMeters")); 

                UseGoogleAccounts = Convert.ToBoolean(iniFile.IniReadValue("General", "LoadGoogleAccounts"));
                UsePtcAccounts = Convert.ToBoolean(iniFile.IniReadValue("General", "LoadPTCAccounts"));
                UseLuckyEggs = Convert.ToBoolean(iniFile.IniReadValue("General", "UseLuckyEggs"));
                UseEggIncubators = Convert.ToBoolean(iniFile.IniReadValue("General", "UseEggIncubators"));
    
                RenamePokemons = Convert.ToBoolean(iniFile.IniReadValue("General", "RenamePokemons"));

                AutoEvolve = Convert.ToBoolean(iniFile.IniReadValue("Evolve", "AutoEvolve"));
                EvolveAllPokemonAboveIV = Convert.ToBoolean(iniFile.IniReadValue("Evolve", "EvolveAllPokemonAboveIV")); 
                EvolveAboveIVValue = float.Parse(iniFile.IniReadValue("Evolve", "EvolveAboveIVValue"));

                AutoTransfer = Convert.ToBoolean(iniFile.IniReadValue("Transfer", "AutoTransfer"));
                KeepMinIVPercentage = float.Parse(iniFile.IniReadValue("Transfer", "KeepMinIVPercentage"), CultureInfo.InvariantCulture);
                KeepMinCP = Convert.ToInt32(iniFile.IniReadValue("Transfer", "KeepMinCP"));
                KeepMinDuplicatePokemon = Convert.ToInt32(iniFile.IniReadValue("Transfer", "KeepMinDuplicatePokemon"));
                RecycleItems = Convert.ToBoolean(iniFile.IniReadValue("Items", "RecycleItems"));
                PrioritizeIVOverCP = Convert.ToBoolean(iniFile.IniReadValue("Transfer", "PrioritizeIVOverCP"));

                DelayBetweenPokemonCatch = Convert.ToInt32(iniFile.IniReadValue("Delays", "AfterCatchInSeconds"));

                if (RecycleItems)
                {
                    try
                    {
                        Write("Loading itemRecycleFilter.ini");
                        IniFile iniItemFile = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "itemRecycleFilter.ini");

                        int PokeBall = Convert.ToInt32(iniItemFile.IniReadValue("Items", "PokeBall"));
                        int GreatBall = Convert.ToInt32(iniItemFile.IniReadValue("Items", "GreatBall"));
                        int UltraBall = Convert.ToInt32(iniItemFile.IniReadValue("Items", "UltraBall"));
                        int MasterBall = Convert.ToInt32(iniItemFile.IniReadValue("Items", "MasterBall"));
                        int Potion = Convert.ToInt32(iniItemFile.IniReadValue("Items", "Potion"));
                        int SuperPotion = Convert.ToInt32(iniItemFile.IniReadValue("Items", "SuperPotion"));
                        int HyperPotion = Convert.ToInt32(iniItemFile.IniReadValue("Items", "HyperPotion"));
                        int MaxPotion = Convert.ToInt32(iniItemFile.IniReadValue("Items", "MaxPotion"));
                        int Revive = Convert.ToInt32(iniItemFile.IniReadValue("Items", "Revive"));
                        int MaxRevive = Convert.ToInt32(iniItemFile.IniReadValue("Items", "MaxRevive"));
                        int LuckyEgg = Convert.ToInt32(iniItemFile.IniReadValue("Items", "LuckyEgg"));
                        int RazzBerry = Convert.ToInt32(iniItemFile.IniReadValue("Items", "RazzBerry"));

                        itemList = new[]
                            {
                                    new KeyValuePair<ItemId, int>(ItemId.ItemUnknown, 0),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemPokeBall, PokeBall),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemGreatBall, GreatBall),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemUltraBall, UltraBall),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemMasterBall, MasterBall),

                                    new KeyValuePair<ItemId, int>(ItemId.ItemPotion, Potion),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemSuperPotion, SuperPotion),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemHyperPotion, HyperPotion),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemMaxPotion, MaxPotion),

                                    new KeyValuePair<ItemId, int>(ItemId.ItemRevive, Revive),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemMaxRevive, MaxRevive),

                                     new KeyValuePair<ItemId, int>(ItemId.ItemLuckyEgg, LuckyEgg),

                                     new KeyValuePair<ItemId, int>(ItemId.ItemIncenseOrdinary, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemIncenseSpicy, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemIncenseCool, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemIncenseFloral, 100),

                                     new KeyValuePair<ItemId, int>(ItemId.ItemTroyDisk, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemXAttack, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemXDefense, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemXMiracle, 100),

                                     new KeyValuePair<ItemId, int>(ItemId.ItemRazzBerry, RazzBerry),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemBlukBerry, 10),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemNanabBerry, 10),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemWeparBerry, 30),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemPinapBerry, 30),

                                     new KeyValuePair<ItemId, int>(ItemId.ItemSpecialCamera, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasicUnlimited, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasic, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemPokemonStorageUpgrade, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemItemStorageUpgrade, 100),
                        };
                    }
                    catch (Exception)
                    {
                        Write("Error while setting RecycleItemsFilter, default will be used.");
                    }
                } else
                {
                    itemList = new[]
    {
                                    new KeyValuePair<ItemId, int>(ItemId.ItemUnknown, 0),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemPokeBall, 25),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemGreatBall, 50),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemUltraBall, 75),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemMasterBall, 200),

                                    new KeyValuePair<ItemId, int>(ItemId.ItemPotion, 0),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemSuperPotion, 0),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemHyperPotion, 50),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemMaxPotion, 70),

                                    new KeyValuePair<ItemId, int>(ItemId.ItemRevive, 0),
                                    new KeyValuePair<ItemId, int>(ItemId.ItemMaxRevive, 50),

                                     new KeyValuePair<ItemId, int>(ItemId.ItemLuckyEgg, 200),

                                     new KeyValuePair<ItemId, int>(ItemId.ItemIncenseOrdinary, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemIncenseSpicy, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemIncenseCool, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemIncenseFloral, 100),

                                     new KeyValuePair<ItemId, int>(ItemId.ItemTroyDisk, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemXAttack, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemXDefense, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemXMiracle, 100),

                                     new KeyValuePair<ItemId, int>(ItemId.ItemRazzBerry, 50),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemBlukBerry, 10),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemNanabBerry, 10),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemWeparBerry, 30),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemPinapBerry, 30),

                                     new KeyValuePair<ItemId, int>(ItemId.ItemSpecialCamera, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasicUnlimited, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasic, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemPokemonStorageUpgrade, 100),
                                     new KeyValuePair<ItemId, int>(ItemId.ItemItemStorageUpgrade, 100),
                        };
                }
            } catch (Exception e)
            {
                Write("You have an invalid settings.ini, please check them." + e.Message, LogLevel.Error);
            }
        }

        public ICollection<KeyValuePair<ItemId, int>> ItemRecycleFilter
        {
            get
            {
                return itemList;
            }
        }
       
        public ICollection<PokemonId> PokemonsNotToTransfer
        {
            get
            {
                //Type of pokemons not to transfer
                var defaultText = new string[] { "Dragonite", "Charizard", "Zapdos", "Snorlax", "Alakhazam", "Mew", "Mewtwo" };
                _pokemonsNotToTransfer = _pokemonsNotToTransfer ?? LoadPokemonList("PokemonsToKeep.txt", defaultText);
                return _pokemonsNotToTransfer;
            }
        }

        private static ICollection<PokemonId> LoadPokemonList(string filename, string[] defaultContent)
        {
            ICollection<PokemonId> result = new List<PokemonId>();
            Func<string, ICollection<PokemonId>> addPokemonToResult = delegate (string pokemonName) {
                PokemonId pokemon;
                if (Enum.TryParse<PokemonId>(pokemonName, out pokemon))
                {
                    result.Add((PokemonId)pokemon);
                }
                return result;
            };

            if (File.Exists(Directory.GetCurrentDirectory() + "\\" + filename))
            {
                var content = string.Empty;
                using (StreamReader reader = new StreamReader(filename))
                {
                    content = reader.ReadToEnd();
                    reader.Close();
                }

                content = Regex.Replace(content, @"\\/\*(.|\n)*?\*\/", ""); //todo: supposed to remove comment blocks


                StringReader tr = new StringReader(content);

                var pokemonName = tr.ReadLine();
                while (pokemonName != null)
                {
                    addPokemonToResult(pokemonName);
                    pokemonName = tr.ReadLine();
                }
            }
            else
            {
                Write($"File: {filename} not found, creating new...", LogLevel.Warning);
                using (var w = File.AppendText(Directory.GetCurrentDirectory() + "\\" + filename))
                {
                    Array.ForEach(defaultContent, x => w.WriteLine(x));
                    Array.ForEach(defaultContent, x => addPokemonToResult(x));
                    w.Close();
                }
            }
            return result;
        }

        public ICollection<PokemonId> PokemonsToEvolve
        {
            get
            {
                //Type of pokemons to evolve
                var defaultText = new string[] { "Zubat", "Pidgey", "Ratata" };
                _pokemonsToEvolve = _pokemonsToEvolve ?? LoadPokemonList("PokemonsToEvolve.txt", defaultText);
                return _pokemonsToEvolve;
            }
        }
    }
}
