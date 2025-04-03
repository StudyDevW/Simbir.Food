using Microsoft.Extensions.Configuration;
using Middleware_Components.DTO.YandexDTO;
using Middleware_Components.Services;
using System.Text.Json;

namespace Middleware_Components.Yandex
{
    public class YandexIntegration : IYandexIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public YandexIntegration(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<YandexCoords?> GetCoordinatesFromAddress(string address)
        {
            try
            {
                string requestUrl = $"https://geocode-maps.yandex.ru/1.x/?apikey={_configuration["YANDEX_BACKEND_API_KEY"]}&geocode={Uri.EscapeDataString("Ульяновск, " + address)}&format=json";
            
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("yandex_geocoder_api_error_request");
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;

                var featureMembers = root
                    .GetProperty("response")
                    .GetProperty("GeoObjectCollection")
                    .GetProperty("featureMember");

                if (featureMembers.GetArrayLength() > 0)
                {
                    string pos = featureMembers[0]
                        .GetProperty("GeoObject")
                        .GetProperty("Point")
                        .GetProperty("pos")
                        .GetString()!;

                    var coordinates = pos.Split(' ');

                    if (coordinates.Length != 2) 
                        throw new Exception("coords_error");

                    double lonVal = double.Parse(coordinates[0]);
                    double latVal = double.Parse(coordinates[1]);

                    return new YandexCoords() { lat = latVal, lon = lonVal };
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return null;
        }
    }
}
