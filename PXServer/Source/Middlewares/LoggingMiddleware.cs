namespace PXServer.Source.Middlewares;


public class LoggingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<LoggingMiddleware> logger;


    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        var remoteIpAddress = context.Connection.RemoteIpAddress?.ToString();
        var route = context.Request.Path;
        this.logger.LogInformation($"Requête reçue. IP: {remoteIpAddress}, Route: {route}");
        await this.next(context);
    }
}