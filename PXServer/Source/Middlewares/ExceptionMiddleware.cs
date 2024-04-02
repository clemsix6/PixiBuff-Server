using System.Text.Json;
using PXServer.Source.Exceptions;


namespace PXServer.Source.Middlewares;


public class ExceptionMiddleware
{
    private readonly RequestDelegate next;


    public ExceptionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }


    public async Task Invoke(HttpContext context)
    {
        try {
            await this.next(context);
        } catch (RouteException ex) {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;

            var result = JsonSerializer.Serialize(
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

            var result = JsonSerializer.Serialize(
                new
                {
                    status = 500,
                    message = ex.Message
                }
            );
            await context.Response.WriteAsync(result);
            throw;
        }
    }
}