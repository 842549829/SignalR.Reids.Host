namespace SignalR.Host;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 注入SignalR
        builder.Services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
            })
            // .AddRedis("localhost:6379");
            .AddStackExchangeRedis("localhost:6379", options =>
        {
            options.Configuration.ChannelPrefix = "WebSocketTest" + Guid.NewGuid();
        });




        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // 注入Hub 集线器方便其他地方调用
        builder.Services.AddSingleton<MessagePushServiceHub>();

        // 跨域设置
        builder.Services.AddCors(options => options.AddPolicy("CorsPolicy", configurePolicy =>
        {
            configurePolicy.AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowCredentials();
        }));


        //builder.Services.AddSingleton(typeof(MessagePushServiceHub));
        var app = builder.Build();

        // 处理权限
        app.Use((context, next) =>
        {
            if (context.Request.Query.TryGetValue("access_token", out var token)
                && context.Request.Path.StartsWithSegments("/hub"))
                context.Request.Headers.Add("token", token);
            return next.Invoke();
        });

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        // 配置集线器路由
        app.MapHub<MessagePushServiceHub>("/hub");

        // 配置跨域中间件
        app.UseCors("CorsPolicy");

        // 启用WebSockets
        app.UseWebSockets();

        // Controller控制器中间件
        app.MapControllers();

        // 配置启动地址
        app.Run();
    }
}