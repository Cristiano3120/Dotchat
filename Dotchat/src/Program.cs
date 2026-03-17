namespace DotchatServer.src;

public static class Program
{
    public static void Main()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        _ = builder.Services.AddControllers();
        _ = builder.Services.AddOpenApi();

        WebApplication app = builder.Build();
    }
}