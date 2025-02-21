namespace TestsBaseLib.Base;

public class TestConfig
{
    public string DatabaseConnectionString { get; set; }
    public string RedisEndPoint { get; set; }
    public string RedisPassword { get; set; }
    public string RSA_PUBLIC_KEY { get; set; }
    public string RSA_PRIVATE_KEY { get; set; }
    public string JwtIssuer { get; set; }
    public string JwtAudience { get; set; }
}
