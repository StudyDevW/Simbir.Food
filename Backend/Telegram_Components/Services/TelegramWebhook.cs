using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram_Components.Interfaces;

namespace Telegram_Components.Services
{
    public class TelegramWebhook : ITelegramWebhook
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public TelegramWebhook(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task WebhookSet(string urlDomain)
        {
            var hookUrl = $"https://api.telegram.org/bot{_configuration["TELEGRAM_TOKEN"]}/setWebhook?url={urlDomain}";

            var response = await _httpClient.GetAsync(hookUrl);

            response.EnsureSuccessStatusCode();
        }

    }
}
