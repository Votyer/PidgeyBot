using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using PokemonGo.RocketAPI.Enums;
using System.Collections.Generic;

namespace PokemonGo.RocketAPI
{
    public interface ISettings
    {
        double DefaultLatitude { get; set; }
        double DefaultLongitude { get; set; }
        double DefaultAltitude { get; set; }
        float KeepMinIVPercentage { get; set; }
        int KeepMinCP { get; set; }
        int KeepMinDuplicatePokemon { get; set; }
        int MaxTravelDistanceInMeters { get; set; }

        double WalkingSpeedInKilometerPerHour { get; set; }
        bool AutoEvolve { get; set; }
        bool PrioritizeIVOverCP { get; set; }
        bool AutoTransfer { get; set; }
        bool RecycleItems { get; set; }
        bool UseGoogleAccounts { get; set; }
        bool UsePtcAccounts { get; set; }
        bool UseLuckyEggs { get; set; }
        int AfterCatchInSeconds { get; set; }
        float EvolveAboveIVValue { get; set; }
        bool EvolveAllPokemonAboveIV { get; set; }
        bool UseHumanWalking { get; set; }

        //WHY
        AuthType AuthType { get; set; }
        string GoogleRefreshToken { get; set; }
        string PtcUsername { get; set; }
        string PtcPassword { get; set; }


        bool KeepPokemonsThatCanEvolve { get; set; }

        bool UseEggIncubators { get; set; }
        bool RenamePokemons { get; set; }

        int DelayBetweenPokemonCatch { get; set; }
        ICollection<KeyValuePair<ItemId, int>> ItemRecycleFilter { get; }
        ICollection<PokemonId> PokemonsNotToTransfer { get; }
        ICollection<PokemonId> PokemonsToEvolve { get; }
    }
}