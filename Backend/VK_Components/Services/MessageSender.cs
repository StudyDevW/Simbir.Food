using System.Text.Json;
using VK_Components.Interfaces;

namespace VK_Components.Services
{
    public class MessageSender : IMessageSender
    {
        private readonly HttpClient _httpClient;
        private readonly string _backendUrl;
        private readonly string _serviceKey;
        private readonly string _apiVersion;

        public MessageSender(string serviceKey, string backendUrl, string apiVersion = "5.199")
        {
            _serviceKey = serviceKey;
            _apiVersion = apiVersion;
            _httpClient = new HttpClient { BaseAddress = new Uri("https://api.vk.com/method/") };
            _backendUrl = backendUrl;
        }

        public async Task Send(string userId, string message)
        {
            await SendNotification(new[] { long.Parse(userId) }, message, null);
        }

        public async Task SendConfirmationRequest(string userId)
        {
            var message = "Подтвердите регистрацию";

            var fragment = $"{_backendUrl}/api/confirm?user_id={userId}";

            await SendNotification(new[] { long.Parse(userId) }, message, fragment);
        }

        private async Task SendNotification(long[] userIds, string message, string fragment)
        {
            if (string.IsNullOrEmpty(message) || message.Length > 254)
            {
                throw new ArgumentException("Message must be between 1 and 254 characters");
            }


            var parameters = new Dictionary<string, string>
            {
                ["user_ids"] = string.Join(",", userIds),
                ["message"] = message,
                ["access_token"] = _serviceKey,
                ["v"] = _apiVersion
            };

            if (!string.IsNullOrEmpty(fragment))
            {
                parameters["fragment"] = fragment;
            }

            var content = new FormUrlEncodedContent(parameters);
            await _httpClient.PostAsync("notifications.sendMessage", content);
        }


    }
}
