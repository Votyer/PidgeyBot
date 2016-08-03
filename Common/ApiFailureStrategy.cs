#region using directives

using System;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Common;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;

#endregion

namespace PidgeyBot.Common
{

    public class ApiFailureStrategy : IApiFailureStrategy
    {
        private readonly PidgeyInstance _session;
        private int _retryCount;

        public ApiFailureStrategy(PidgeyInstance session)
        {
            _session = session;
        }

        public async Task<ApiOperation> HandleApiFailure()
        {
            if (_retryCount == 11)
                return ApiOperation.Abort;

            await Task.Delay(500);
            _retryCount++;

            if (_retryCount % 5 == 0)
            {
                DoLogin();
            }

            return ApiOperation.Retry;
        }

        public void HandleApiSuccess()
        {
            _retryCount = 0;
        }

        private async void DoLogin()
        {
            switch (_session._client.Settings.AuthType)
            {
                case AuthType.Google:
                    System.Console.WriteLine("Yo, mail: " + _session._clientSettings.PtcUsername + " pass: " + _session._clientSettings.PtcPassword);
                    await _session._client.Login.DoGoogleLogin(_session._clientSettings.PtcUsername, _session._clientSettings.PtcPassword);
                    break;
                case AuthType.Ptc:
                    await _session._client.Login.DoPtcLogin(_session._clientSettings.PtcUsername, _session._clientSettings.PtcPassword);
                    break;
            }
        }
    }
}