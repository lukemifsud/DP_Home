using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); // required for Cloud Run
});

// Load your ocelot.json and enable Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

var app = builder.Build();

app.MapGet("/", () => "API Gateway is alive!");

builder.Logging.AddConsole();

try
{
    await app.UseOcelot();
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("❌ Ocelot startup failed:");
    Console.WriteLine(ex.Message);
}