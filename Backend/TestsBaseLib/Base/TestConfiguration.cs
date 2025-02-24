using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Reflection;
using System.Text.Json;

namespace TestsBaseLib.Base;

public class TestConfiguration
{
    public static IConfiguration GetConfiguration()
    {
        var config = new ConfigurationBuilder();
        config.AddJsonFile("settings.json");
        return config.Build();
    }
}
