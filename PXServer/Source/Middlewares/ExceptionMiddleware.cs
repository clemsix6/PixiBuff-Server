using Newtonsoft.Json;
using NLog;
using PXServer.Source.Exceptions;


namespace PXServer.Source.Middlewares;


public class ExceptionMiddleware
{
    private readonly Logger logger;
    private readonly RequestDelegate next;


    public ExceptionMiddleware(RequestDelegate next)
    {
        this.logger = LogManager.GetCurrentClassLogger();
        this.next = next;
    }


    public async Task Invoke(HttpContext context)
    {
        try {
            await this.next(context);
        } catch (ServerException ex) {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;

            var result = JsonConvert.SerializeObject(
                new
                {
                    status = ex.StatusCode,
                    message = ex.Message
                }
            );
            await context.Response.WriteAsync(result);
        } catch (Exception ex) {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var result = JsonConvert.SerializeObject(
                new
                {
                    status = 500,
                    message = ex.Message
                }
            );
            await context.Response.WriteAsync(result);
            this.logger.Error(ex.Message + ex.StackTrace);
        }
    }
}