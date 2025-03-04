using DotNetEnv;
using DotNetEnv.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Middleware_Components.Broker;
using Middleware_Components.Cache;
using Middleware_Components.Services;
using ORM_Components.Interfaces;
using ORM_Components.MapsterConfigs;
using ORM_Components.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());

            builder.Services.AddDbContext<DataContext>(options =>
            {
                var connectString = builder.Configuration["DATABASE_CONNECT"];

                if (connectString != null)
                    options.UseNpgsql(connectString);
            });

            builder.Services.AddSingleton<IAutoMigrationService, AutoMigrationService>();

            builder.Services.AddSingleton<ICacheService, CacheSDK>();

            builder.Services.AddSingleton<IMailSender, MailSender>();

            builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

            builder.Services.AddHostedService<RabbitMQListenerService>();

            var app = builder.Build();

            using (var serviceScope = app.Services.CreateScope())
            {
                var migrations = serviceScope.ServiceProvider.GetService<IAutoMigrationService>();

                if (migrations != null)
                    await migrations.EnsureDatabaseInitializedAsync();
            }

            await app.RunAsync();
        }
    }
}
