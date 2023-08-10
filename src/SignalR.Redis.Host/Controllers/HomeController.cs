using SignalR.Redis.Host.Hubs;

namespace SignalR.Redis.Host.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;
    private readonly MessagePushServiceHub _messagePushService;

    public HomeController(ILogger<HomeController> logger,
        MessagePushServiceHub messagePushService)
    {
        _logger = logger;
        _messagePushService = messagePushService;
    }

    [HttpGet]
    public async Task<string> GetAsync()
    {
        await _messagePushService.PushAsync("aaa", "xxxxx", new[] { "U:123" });

        return "OK";
    }
}