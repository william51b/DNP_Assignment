using RepositoryContracts;

namespace WebAPI.Middleware;

public class GlobalExceptionHandlerMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> logger;

    public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException e)
        {
            await WriteAsync(context, StatusCodes.Status404NotFound, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unhandled exception");
            await WriteAsync(context, StatusCodes.Status500InternalServerError, e.Message);
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = message });
    }
}