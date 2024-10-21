using CoffeeShopPubSub.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var services = new ServiceCollection();

    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .WriteTo.Console()
        .CreateLogger();

    services.AddSingleton<IWorker, Worker>();

    services.AddSingleton<Barista>();

    services.AddSingleton(sp => new Customer[]
    {
        new("Zach", DrinkingSpeed.CaffeineHigh),
        new("Andy", DrinkingSpeed.Leisurely),
        new("Kate", DrinkingSpeed.PrefersTea)
    });

    var serviceProvider = services.BuildServiceProvider();

    var cts = new CancellationTokenSource();

    var worker = serviceProvider.GetRequiredService<IWorker>();

    await worker.RunAsync(cts);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}