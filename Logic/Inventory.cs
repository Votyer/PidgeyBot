﻿using PidgeyBot.Utils;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using PokemonGo.RocketAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PidgeyBot.Logic
{
    public class Inventory
    {
        private readonly PidgeyInstance _client;
        private GetInventoryResponse _cachedInventory;
        private DateTime _lastRefresh;

        public Inventory(PidgeyInstance client)
        {
            _client = client;
        }

        public async Task DeletePokemonFromInvById(ulong id)
        {
            var inventory = await GetCachedInventory();
            var pokemon =
                inventory.InventoryDelta.InventoryItems.FirstOrDefault(
                    i => i.InventoryItemData.PokemonData != null && i.InventoryItemData.PokemonData.Id == id);
            if (pokemon != null)
                inventory.InventoryDelta.InventoryItems.Remove(pokemon);
        }

        private async Task<GetInventoryResponse> GetCachedInventory()
        {
            var now = DateTime.UtcNow;

            if (_lastRefresh.AddSeconds(30).Ticks > now.Ticks)
            {
                return _cachedInventory;
            }
            return await RefreshCachedInventory();
        }

        public async Task<IEnumerable<PokemonData>> GetDuplicatePokemonToTransfer(
            bool keepPokemonsThatCanEvolve = false, bool prioritizeIVoverCp = false,
            IEnumerable<PokemonId> filter = null)
        {
            var myPokemon = await GetPokemons();

            var pokemonList =
                myPokemon.Where(
                    p => p.DeployedFortId == string.Empty && PokemonInfo.CalculatePokemonPerfection(p) < 100 &&
                         (p.Favorite == 0 && p.Cp < _client._clientSettings.KeepMinCP || PokemonInfo.CalculatePokemonPerfection(p) < _client._clientSettings.KeepMinIVPercentage))
                    .ToList();
            if (filter != null)
            {
                pokemonList = pokemonList.Where(p => !filter.Contains(p.PokemonId)).ToList();
            }
            if (keepPokemonsThatCanEvolve)
            {
                var results = new List<PokemonData>();
                var pokemonsThatCanBeTransfered = pokemonList.GroupBy(p => p.PokemonId)
                    .Where(x => x.Count() > _client._clientSettings.KeepMinDuplicatePokemon).ToList();

                var myPokemonSettings = await GetPokemonSettings();
                var pokemonSettings = myPokemonSettings.ToList();

                var myPokemonFamilies = await GetPokemonFamilies();
                var pokemonFamilies = myPokemonFamilies.ToArray();

                foreach (var pokemon in pokemonsThatCanBeTransfered)
                {
                    var settings = pokemonSettings.Single(x => x.PokemonId == pokemon.Key);
                    var familyCandy = pokemonFamilies.Single(x => settings.FamilyId == x.FamilyId);
                    var amountToSkip = _client._clientSettings.KeepMinDuplicatePokemon;

                    if (settings.CandyToEvolve > 0)
                    {
                        var amountPossible = familyCandy.Candy_ / settings.CandyToEvolve;
                        if (amountPossible > amountToSkip)
                            amountToSkip = amountPossible;
                    }

                    if (prioritizeIVoverCp)
                    {
                        results.AddRange(pokemonList.Where(x => x.PokemonId == pokemon.Key)
                            .OrderByDescending(PokemonInfo.CalculatePokemonPerfection)
                            .ThenBy(n => n.StaminaMax)
                            .Skip(amountToSkip)
                            .ToList());
                    }
                    else
                    {
                        results.AddRange(pokemonList.Where(x => x.PokemonId == pokemon.Key)
                            .OrderByDescending(x => x.Cp)
                            .ThenBy(n => n.StaminaMax)
                            .Skip(amountToSkip)
                            .ToList());
                    }
                }

                return results;
            }
            if (prioritizeIVoverCp)
            {
                return pokemonList
                    .GroupBy(p => p.PokemonId)
                    .Where(x => x.Count() > 0)
                    .SelectMany(
                        p =>
                            p.OrderByDescending(PokemonInfo.CalculatePokemonPerfection)
                                .ThenBy(n => n.StaminaMax)
                                .Skip(_client._clientSettings.KeepMinDuplicatePokemon)
                                .ToList());
            }
            return pokemonList
                .GroupBy(p => p.PokemonId)
                .Where(x => x.Count() > 0)
                .SelectMany(
                    p =>
                        p.OrderByDescending(x => x.Cp)
                            .ThenBy(n => n.StaminaMax)
                            .Skip(_client._clientSettings.KeepMinDuplicatePokemon)
                            .ToList());
        }

        public async Task<PokemonData> GetHighestPokemonOfTypeByCp(PokemonData pokemon)
        {
            var myPokemon = await GetPokemons();
            var pokemons = myPokemon.ToList();
            return pokemons.Where(x => x.PokemonId == pokemon.PokemonId)
                .OrderByDescending(x => x.Cp)
                .FirstOrDefault();
        }

        public async Task<PokemonData> GetHighestPokemonOfTypeByIv(PokemonData pokemon)
        {
            var myPokemon = await GetPokemons();
            var pokemons = myPokemon.ToList();
            return pokemons.Where(x => x.PokemonId == pokemon.PokemonId)
                .OrderByDescending(PokemonInfo.CalculatePokemonPerfection)
                .FirstOrDefault();
        }

        public async Task<IEnumerable<PokemonData>> GetHighestsCp(int limit)
        {
            var myPokemon = await GetPokemons();
            var pokemons = myPokemon.ToList();
            return pokemons.OrderByDescending(x => x.Cp).ThenBy(n => n.StaminaMax).Take(limit);
        }

        public async Task<IEnumerable<PokemonData>> GetHighestsPerfect(int limit)
        {
            var myPokemon = await GetPokemons();
            var pokemons = myPokemon.ToList();
            return pokemons.OrderByDescending(PokemonInfo.CalculatePokemonPerfection).Take(limit);
        }

        public async Task<IEnumerable<PokemonData>> GetEggs()
        {
            var inventory = await GetCachedInventory();
            return
                inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData)
                    .Where(p => p != null && p.IsEgg);
        }

        public async Task<IEnumerable<EggIncubator>> GetEggIncubators()
        {
            var inventory = await GetCachedInventory();
            return
                inventory.InventoryDelta.InventoryItems
                .Where(x => x.InventoryItemData.EggIncubators != null)
                .SelectMany(i => i.InventoryItemData.EggIncubators.EggIncubator)
                .Where(i => i != null);
        }

        public async Task<int> GetItemAmountByType(ItemId type)
        {
            var pokeballs = await GetItems();
            return pokeballs.FirstOrDefault(i => i.ItemId == type)?.Count ?? 0;
        }

        public async Task<IEnumerable<ItemData>> GetItems()
        {
            var inventory = await GetCachedInventory();
            return inventory.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.Item)
                .Where(p => p != null);
        }

        public async Task<IEnumerable<ItemData>> GetItemsToRecycle(ISettings settings)
        {
            var myItems = await GetItems();

            return myItems
                .Where(x => _client._clientSettings.ItemRecycleFilter.Any(f => f.Key == x.ItemId && x.Count > f.Value))
                .Select(
                    x =>
                        new ItemData
                        {
                            ItemId = x.ItemId,
                            Count = x.Count - _client._clientSettings.ItemRecycleFilter.Single(f => f.Key == x.ItemId).Value,
                            Unseen = x.Unseen
                        });
        }

        public async Task<IEnumerable<PlayerStats>> GetPlayerStats()
        {
            var inventory = await GetCachedInventory();
            return inventory.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.PlayerStats)
                .Where(p => p != null);
        }

        public async Task<List<Candy>> GetPokemonFamilies()
        {
            var inventory = await GetCachedInventory();

            var families = from item in inventory.InventoryDelta.InventoryItems
                           where item.InventoryItemData?.Candy != null
                           where item.InventoryItemData?.Candy.FamilyId != PokemonFamilyId.FamilyUnset
                           group item by item.InventoryItemData?.Candy.FamilyId into family
                           select new Candy
                           {
                               FamilyId = family.First().InventoryItemData.Candy.FamilyId,
                               Candy_ = family.First().InventoryItemData.Candy.Candy_
                           };


            return families.ToList();
        }

        public async Task<IEnumerable<PokemonData>> GetPokemons()
        {
            var inventory = await GetCachedInventory();
            return
                inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData)
                    .Where(p => p != null && p.PokemonId > 0);
        }

        public async Task<IEnumerable<PokemonSettings>> GetPokemonSettings()
        {
            var templates = await _client._client.Download.GetItemTemplates();
            return
                templates.ItemTemplates.Select(i => i.PokemonSettings)
                    .Where(p => p != null && p.FamilyId != PokemonFamilyId.FamilyUnset);
        }

        public async Task<IEnumerable<PokemonData>> GetPokemonToEvolve(IEnumerable<PokemonId> filter = null)
        {
            var myPokemons = await GetPokemons();
            myPokemons = myPokemons.Where(p => p.DeployedFortId == string.Empty).OrderByDescending(p => p.Cp);
            //Don't evolve pokemon in gyms
            if (filter != null)
            {
                myPokemons = myPokemons.Where(p => filter.Contains(p.PokemonId));
            }
            var pokemons = myPokemons.ToList();

            var myPokemonSettings = await GetPokemonSettings();
            var pokemonSettings = myPokemonSettings.ToList();

            var myPokemonFamilies = await GetPokemonFamilies();
            var pokemonFamilies = myPokemonFamilies.ToArray();

            var pokemonToEvolve = new List<PokemonData>();
            foreach (var pokemon in pokemons)
            {
                var settings = pokemonSettings.Single(x => x.PokemonId == pokemon.PokemonId);
                var familyCandy = pokemonFamilies.Single(x => settings.FamilyId == x.FamilyId);

                //Don't evolve if we can't evolve it
                if (settings.EvolutionIds.Count == 0)
                    continue;

                var pokemonCandyNeededAlready =
                    pokemonToEvolve.Count(
                        p => pokemonSettings.Single(x => x.PokemonId == p.PokemonId).FamilyId == settings.FamilyId) *
                    settings.CandyToEvolve;

                if (_client._clientSettings.EvolveAllPokemonAboveIV)
                {
                    if (PokemonInfo.CalculatePokemonPerfection(pokemon) >= _client._clientSettings.EvolveAboveIVValue &&
                        familyCandy.Candy_ - pokemonCandyNeededAlready > settings.CandyToEvolve)
                    {
                        pokemonToEvolve.Add(pokemon);
                    }
                }
                else
                {
                    if (familyCandy.Candy_ - pokemonCandyNeededAlready > settings.CandyToEvolve)
                    {
                        pokemonToEvolve.Add(pokemon);
                    }
                }
            }

            return pokemonToEvolve;
        }

        public async Task<GetInventoryResponse> RefreshCachedInventory()
        {
            var now = DateTime.UtcNow;
            var ss = new SemaphoreSlim(10);

            await ss.WaitAsync();
            try
            {
                _lastRefresh = now;
                _cachedInventory = await _client._client.Inventory.GetInventory();
                return _cachedInventory;
            }
            finally
            {
                ss.Release();
            }
        }
    }
}