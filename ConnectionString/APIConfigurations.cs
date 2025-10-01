namespace MinimalApi.ConnectionString;

public class APIConfigurations
{
    public const string Section = "APIConfigurations";
    public required int WeatherForecastsToReturn { get; set; }
}