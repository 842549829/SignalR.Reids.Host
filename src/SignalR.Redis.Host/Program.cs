using SignalR.Redis.Host.Hubs;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace SignalR.Redis.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 跨域设置
            builder.Services.AddCors(options => options.AddPolicy("CorsPolicy", configurePolicy =>
            {
                configurePolicy.AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowCredentials();
            }));
            builder.Services.AddSignalR(hubOptions =>
                {
                    hubOptions.EnableDetailedErrors = true;
                })
                .AddStackExchangeRedis("localhost:6379", options =>
                {
                    options.Configuration.ChannelPrefix = "WebSocketTest";
                });

            builder.Services.AddControllers();

            // 注入Hub 集线器方便其他地方调用
            builder.Services.AddSingleton<MessagePushServiceHub>();

            var app = builder.Build();

            // 配置集线器路由
            app.MapHub<MessagePushServiceHub>("/hub");
            

            // 配置跨域中间件
            app.UseCors("CorsPolicy");

            app.MapControllers();

            app.Run();
        }
    }
}