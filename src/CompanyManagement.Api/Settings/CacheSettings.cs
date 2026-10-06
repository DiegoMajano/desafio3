namespace CompanyManagement.Api.Settings;

public class CacheSettings
{
    public string InstanceName { get; set; } = "CompanyManagement:";
    public int ExpirationMinutes { get; set; } = 10;
}
