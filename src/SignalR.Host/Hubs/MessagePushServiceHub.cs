using Microsoft.AspNetCore.SignalR.StackExchangeRedis;

namespace SignalR.Host.Hubs;

public class MessagePushServiceHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // 从Token中获取
        var userId = "123";

        // 从Token中获取
        var organization = Guid.NewGuid();

        // 添加到用户自己的通知组
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(MessageScopingProviderName.User, userId.ToString()));

        // 添加到自己所属组织的通知组
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(MessageScopingProviderName.Organization, organization.ToString()));

        await base.OnConnectedAsync();
    }

    private static string GetGroupName(string providerName, string providerKey) => $"{providerName}:{providerKey}";


    // 推送到客户端消息
    public async Task PushAsync(
        string title,
        string content,
        string[] scopes,
        string id = null,
        Guid? sendUserId = null,
        string sendUserName = null)
    {
        if (Clients == null)
        {
            Console.WriteLine("客户端未连接成功");

        }
        else
        {
            await Clients.Groups(scopes)
                .SendAsync("MessageNotification", new
                {
                    Id = id,
                    Title = title,
                    Content = content,
                    SendTime = DateTime.Now,
                    SendUserId = sendUserId,
                    SendUserName = sendUserName
                });
        }

    }


    // 测试接收客户端信息
    public async Task SendMessage(string message)
    {
        await Clients.All
            .SendAsync("MessageNotification", new
            {
                Title = "title",
                Content = message
            });
    }
}