using SignalR.Host.Hubs;

namespace SignalR.Host.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly MessagePushServiceHub _messagePushService;

    public WeatherForecastController(ILogger<WeatherForecastController> logger,
        MessagePushServiceHub messagePushService)
    {
        _logger = logger;
        _messagePushService = messagePushService;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<string> GetAsync()
    {
        await _messagePushService.PushAsync("aaa", "xxxxx", new[] { "U:123" });

        return "OK";
    }
}