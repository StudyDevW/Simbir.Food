using Middleware_Components.DTO.YandexDTO;

namespace Middleware_Components.Services
{
    public interface IYandexIntegrationService
    {
        public Task<YandexCoords?> GetCoordinatesFromAddress(string address);
    }
}
