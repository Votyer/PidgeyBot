using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Common
{
    public enum ApiOperation
    {
        Retry,
        Abort
    }

    public interface IApiFailureStrategy
    {
        Task<ApiOperation> HandleApiFailure();

        void HandleApiSuccess();
    }
}
