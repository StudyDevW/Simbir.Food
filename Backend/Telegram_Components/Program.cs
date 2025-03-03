using DotNetEnv;
using DotNetEnv.Configuration;
using Telegram_Components.Interfaces;
using Telegram_Components.Services;
using Telegram.Bot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ORM_Components;
using Middleware_Components.Services;
using Middleware_Components.Cache;

namespace Telegram_Components
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());

            builder.Services.AddDbContext<DataContext>(options =>
            {
                var connectString = builder.Configuration["DATABASE_CONNECT"];

                if (connectString != null)
                    options.UseNpgsql(connectString, b => b.MigrationsAssembly("Telegram_Components"));
            });

            builder.Services.AddHttpClient<ITelegramWebhook, TelegramWebhook>(o =>
            {
                o.Timeout = TimeSpan.FromSeconds(2);
            });

            builder.Services.AddSingleton<ITelegramBotClient>(
                new TelegramBotClient(builder.Configuration["TELEGRAM_TOKEN"])
            );

            builder.Services.AddScoped<IDatabaseOperations, DatabaseOperations>();

            builder.Services.AddScoped<ICacheService, CacheSDK>();

            builder.Services.AddScoped<IMessageReceiver, MessageReceiver>();

            builder.Services.ConfigureTelegramBotMvc();

            var app = builder.Build();

            using (var serviceScope = app.Services.CreateScope())
            {
                var telegram = serviceScope.ServiceProvider.GetService<ITelegramWebhook>();

                if (telegram != null)
                    await telegram.WebhookSet(builder.Configuration["TELEGRAM_PROVIDER"]);

            }

            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }

    }
}
