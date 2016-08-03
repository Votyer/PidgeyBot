#region using directives

using System;
using System.Threading.Tasks;
using POGOProtos.Networking.Envelopes;
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

        public async Task<ApiOperation> HandleApiFailure(RequestEnvelope request, ResponseEnvelope response)
        {
            if (_retryCount == 11)
                return ApiOperation.Abort;

            await Task.Delay(500);
            _retryCount++;

            if (_retryCount % 5 == 0)
            {
                await _session._client.Login.DoLogin();
            }

            return ApiOperation.Retry;
        }

        public void HandleApiSuccess(RequestEnvelope request, ResponseEnvelope response)
        {
            _retryCount = 0;
        }
    }
}