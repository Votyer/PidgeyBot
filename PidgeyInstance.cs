using PidgeyBot.Common;
using PidgeyBot.Logic;
using PidgeyBot.Utils;
using PoGo.NecroBot.Logic.Tasks;
using POGOProtos.Enums;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PidgeyBot.Utils.Logger;

namespace PidgeyBot
{
    public class PidgeyInstance
    {
        public readonly Client _client;
        public Settings _clientSettings;
        public Inventory _inventory;
        public Navigation _navigation;
        public Statistics _stats;
        public AuthType _authType;
        public string _username;
        public string _password;
        public string _trainerName = "";
        //private GetPlayerResponse _playerProfile;

        public PidgeyInstance(Settings clientSettings, AuthType authType = AuthType.Google, string username = null, string password = null, double customLat = 0, double customLong = 0, Statistics stats = null)
        {
            _clientSettings = clientSettings;
            if (customLat != 0)
                _clientSettings.DefaultLatitude = customLat;
            if (customLong != 0)
                _clientSettings.DefaultLongitude = customLong;

            switch (authType)
            {
                case AuthType.Google:
                    _clientSettings.GoogleUsername = username;
                    _clientSettings.GooglePassword = password;
                    _clientSettings.AuthType = AuthType.Google;
                    break;
                case AuthType.Ptc:
                    _clientSettings.PtcUsername = username;
                    _clientSettings.PtcPassword = password;
                    _clientSettings.AuthType = AuthType.Ptc;
                    break;
            }

            _client = new Client(_clientSettings, new ApiFailureStrategy(this));
            _navigation = new Navigation(this);
            _inventory = new Inventory(this);

            if (stats == null)
                _stats = new Statistics(null);
            else
                _stats = stats;
        }

        public async Task Execute()
        {
            //Logger.Write($"Starting Execute on login server: {_authType}", LogLevel.Info, _trainerName, _authType);
            while (true)
            {
                try
                {
                    await _client.Login.DoLogin();
                    var profile = await _client.Player.GetPlayer(); 
                    _trainerName = profile.PlayerData.Username;
                    Logger.Write($"Logged in as {_trainerName}", LogLevel.Info, _trainerName, _authType);
                    await PostLoginExecute();
                }
                catch (AccessTokenExpiredException)
                {
                    Logger.Write($"Access token expired, trying to relog. (1)", LogLevel.Info, _trainerName, _authType);
                    PidgeyInstance instance = new PidgeyInstance(_clientSettings, _authType, _clientSettings.PtcUsername, _clientSettings.PtcUsername, _clientSettings.DefaultLatitude, _clientSettings.DefaultLongitude, _stats);
                    Task.Run(() => instance.Execute());
                }
                await Task.Delay(10000);
            }
        }

        public async Task PostLoginExecute()
        {
            bool expired = false;
            while (!expired)
            {
                try
                {
                    var profile = await _client.Player.GetPlayer();
                    _trainerName = profile.PlayerData.Username;
                    Logger.Write($"Teleport to Coords: { _clientSettings.DefaultLatitude } / { _clientSettings.DefaultLongitude }", LogLevel.Info, _trainerName, _authType);

                    if(_clientSettings.RenamePokemons)
                        await RenamePokemonTask.Execute(this);

                    if (_clientSettings.EvolveAllPokemonAboveIV || _clientSettings.AutoEvolve)
                        await EvolvePokemonTask.Execute(this);

                    if (_clientSettings.AutoTransfer)
                        await TransferDuplicatePokemonTask.Execute(this);

                    if(_clientSettings.RecycleItems)
                        await RecycleItemsTask.Execute(this);

                    if (_clientSettings.UseEggIncubators)
                        await UseIncubatorsTask.Execute(this);

                    await CatchNearbyPokemonsTask.Execute(this);
                    await FarmPokestopsTask.Execute(this);

                    await Task.Delay(2000);
                }
                catch (AccessTokenExpiredException)
                {
                    expired = true;
                    Logger.Write($"Access token expired, attempt to relog. (2)", LogLevel.Info, _trainerName, _authType);
                    PidgeyInstance instance = new PidgeyInstance(_clientSettings, _authType, _clientSettings.PtcUsername, _clientSettings.PtcUsername, _clientSettings.DefaultLatitude, _clientSettings.DefaultLongitude, _stats);
                    Task.Run(() => instance.Execute());
                }
                catch (InvalidResponseException)
                {
                    expired = true;
                    Logger.Write($"Access token expired, attempt to relog. (3)", LogLevel.Info, _trainerName, _authType);
                    PidgeyInstance instance = new PidgeyInstance(_clientSettings, _authType, _clientSettings.PtcUsername, _clientSettings.PtcUsername, _clientSettings.DefaultLatitude, _clientSettings.DefaultLongitude, _stats);
                    Task.Run(() => instance.Execute());
                }
                catch (Exception ex)
                {
                    Logger.Write($"Exception: {ex}", LogLevel.Error, _trainerName, _authType);
                }
            }
        }
    }
}
