using CourierAPI.Contracts;
using CourierAPI.Service;
using DotNetEnv;
using DotNetEnv.Configuration;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Middleware_Components.Broker;
using Middleware_Components.Cache;
using Middleware_Components.JWT;
using Middleware_Components.Services;
using ORM_Components;
using ORM_Components.Interfaces;
using ORM_Components.MapsterConfigs;
using ORM_Components.Services;
using ORM_Components.Validators.CourierValidators;
using RestaurantAPI.Utility;
using System.Reflection;
using System.Security.Cryptography;
using Telegram_Components.Interfaces;
using Telegram_Components.Services;

namespace CourierAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            //Переменные окружения
            builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());

            var securityScheme = new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Authorize accessToken",
            };

            var securityReq = new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            };

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(o =>
            {
                o.AddSecurityDefinition("Bearer", securityScheme);
                o.AddSecurityRequirement(securityReq);

                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Courier API",
                    Description = "Simbir.Food, Practice"
                });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = "Asymmetric";
                o.DefaultChallengeScheme = "Asymmetric";
                o.DefaultScheme = "Asymmetric";

            }).AddJwtBearer("Asymmetric", o =>
            {
                var rsa = RSA.Create();

                var jwtSdk = new JwtSDK();

                rsa.ImportFromPem(builder.Configuration["RSA_PUBLIC_KEY"]);

                o.IncludeErrorDetails = true;
                o.RequireHttpsMetadata = false;
                o.SaveToken = false;

                TokenValidationParameters tk_valid = new TokenValidationParameters
                {
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    RequireSignedTokens = true,
                    ValidateLifetime = false,
                    RequireExpirationTime = false //#
                };

                o.TokenValidationParameters = tk_valid;
            });

            builder.Services.AddAuthorization();

            builder.Services.AddDbContext<DataContext>(options =>
            {
                var connectString = builder.Configuration["DATABASE_CONNECT"];

                if (connectString != null)
                    options.UseNpgsql(connectString, b => b.MigrationsAssembly("ORM_Components"));
            });

            builder.Services.AddScoped<IJwtService, JwtSDK>();

            builder.Services.AddScoped<ICacheService, CacheSDK>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin",
                    builder => builder.WithOrigins(
                        "http://localhost:4001", 
                        "http://localhost",
                        "https://shockingly-unique-walleye.cloudpub.ru")
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });

            builder.Services.AddSingleton<IMessageSender>(
                 new MessageSender(builder.Configuration["TELEGRAM_TOKEN"])
             );

            builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

            builder.Services.AddScoped<OrderConfig>();

            builder.Services.AddScoped<ICourierService, CourierService>();

            builder.Services.AddSingleton<IMailSender, MailSender>();

            builder.Services.AddFluentValidationAutoValidation();

            builder.Services.AddValidatorsFromAssemblyContaining<CourierValidatorDtoForCreate>();

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            app.UseCors("AllowOrigin");

            app.UseRouting();

            app.UseForwardedHeaders();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Courier API");
                c.RoutePrefix = "ui-swagger";
            });

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/")
                {
                    context.Response.Redirect("/ui-swagger/");
                }
                else
                {
                    await next();
                }
            });

            await app.RunAsync();
        }
    }
}