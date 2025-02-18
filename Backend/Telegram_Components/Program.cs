using DotNetEnv;
using DotNetEnv.Configuration;
using Telegram_Components.Interfaces;
using Telegram_Components.Services;

namespace Telegram_Components
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());

            builder.Services.AddHttpClient<ITelegramWebhook, TelegramWebhook>(o =>
            {
                o.Timeout = TimeSpan.FromSeconds(30);
            });

            var app = builder.Build();

            using (var serviceScope = app.Services.CreateScope())
            {
                var telegram = serviceScope.ServiceProvider.GetService<ITelegramWebhook>();

                if (telegram != null)
                    await telegram.WebhookSet(builder.Configuration["TELEGRAM_PROVIDER"]);
            }

            await app.RunAsync();
        }
    }
}
