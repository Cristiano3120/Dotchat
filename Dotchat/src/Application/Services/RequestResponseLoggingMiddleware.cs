using System.Text;

namespace DotchatServer.src.Application.Services;

public class RequestResponseLoggingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        string requestBody = string.Empty;
        if (context.Request.ContentLength > 0)
        {
            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        Log.Information("Request: {Method} {Path} | Body: {Body}", context.Request.Method, context.Request.Path, requestBody);

        Stream originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        await _next(context);

        _ = responseBodyStream.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
        _ = responseBodyStream.Seek(0, SeekOrigin.Begin);

        Log.Information("Response: {StatusCode} {Path} | Body: {Body}", context.Response.StatusCode, context.Request.Path, responseBody);

        await responseBodyStream.CopyToAsync(originalBodyStream);
    }
}