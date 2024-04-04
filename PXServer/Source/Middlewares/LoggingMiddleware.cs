using NLog;
using LogLevel = NLog.LogLevel;


namespace PXServer.Source.Middlewares;


public class LoggingMiddleware
{
    private readonly Logger logger;
    private readonly RequestDelegate next;


    public LoggingMiddleware(RequestDelegate next)
    {
        this.logger = LogManager.GetCurrentClassLogger();
        this.next = next;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        var ipv4 = context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "??";
        var remoteIp = $"{ipv4}:{context.Connection.RemotePort}";
        var route = context.Request.Path;

        var message = $"({remoteIp}) > \"{route}\"";
        var logEventInfo = LogEventInfo.Create(LogLevel.Info, this.logger.Name, message);
        logEventInfo.Properties["IsRequestLog"] = true;
        this.logger.Log(logEventInfo);

        await this.next(context);
    }
}