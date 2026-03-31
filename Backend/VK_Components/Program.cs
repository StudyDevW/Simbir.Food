using DotNetEnv;
using DotNetEnv.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ORM_Components;
using Middleware_Components.Services;
using Middleware_Components.Cache;
using VK_Components.Interfaces;
using VK_Components.Services;

namespace VK_Components
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
                    options.UseNpgsql(connectString, b => b.MigrationsAssembly("VK_Components"));
            });


            builder.Services.AddSingleton<IMessageSender>(sp =>
                new MessageSender(
                    builder.Configuration["VK_SERVICE_KEY"],
                    builder.Configuration["VK_BACKEND_URL"]
                ));

            builder.Services.AddScoped<IDatabaseOperations, DatabaseOperations>();

            builder.Services.AddScoped<ICacheService, CacheSDK>();

            var app = builder.Build();

            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }

    }
}
