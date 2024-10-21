using System.Threading.Channels;

namespace CoffeeShopPubSub.Services;

public sealed class Barista
{
    private readonly Random _random = new();
    private readonly int _maxOrders = 30;

    public async Task StartMakingCoffeeAsync(
        ChannelWriter<string> writer, 
        CancellationToken ct)
    {
        int orderCount = 0;

        try
        {
            while (!ct.IsCancellationRequested && orderCount < _maxOrders)
            {
                // coffee every 100ms to 500ms (2Hz to 10Hz)
                int productionRate = _random.Next(100, 500);
                await Task.Delay(productionRate, ct);

                string coffeeOrder = $"Coffee #{orderCount}";

                if (writer.TryWrite(coffeeOrder))
                {
                    // coffee tray has space
                    Console.WriteLine($"Barista: {coffeeOrder} is ready!");
                }
                else
                {
                    Console.WriteLine("Barista is waiting due to back-pressure...");

                    // coffee is full - wait for customers to take more
                    await writer.WaitToWriteAsync(ct);

                    Console.WriteLine($"Barista: {coffeeOrder} is cold but ready!");
                    await writer.WriteAsync(coffeeOrder, ct);
                }

                orderCount++;
            }
        }
        finally
        {
            writer.Complete();
            Console.WriteLine("Barista: No more coffee will be made, the shop is closing.");
        }
    }
}
