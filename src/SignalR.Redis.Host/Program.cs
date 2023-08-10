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

            // ��������
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

            // ע��Hub ���������������ط�����
            builder.Services.AddSingleton<MessagePushServiceHub>();

            var app = builder.Build();

            // ���ü�����·��
            app.MapHub<MessagePushServiceHub>("/hub");
            

            // ���ÿ����м��
            app.UseCors("CorsPolicy");

            app.MapControllers();

            app.Run();
        }
    }
}