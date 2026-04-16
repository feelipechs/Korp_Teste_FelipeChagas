using System.Net;
using System.Text.Json;

namespace BillingService.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = JsonSerializer.Serialize(new
        {
            status = 500,
            message = "Ocorreu um erro interno. Tente novamente.",
            detail = ex.Message
        });

        return context.Response.WriteAsync(response);
    }
}