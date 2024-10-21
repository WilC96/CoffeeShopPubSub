using System.Threading.Channels;

namespace CoffeeShopPubSub.Services;

public interface IWorker
{
    public Task RunAsync(CancellationTokenSource cts);
}

public sealed class Worker : IWorker
{
    private readonly Barista _barista;
    private readonly Customer[] _customers;

    public Worker(
        Barista barista,
        Customer[] customers)
    {
        _barista = barista;
        _customers = customers;
    }

    public async Task RunAsync(CancellationTokenSource cts)
    {
        // create channel with buffer size
        var channel = Channel.CreateBounded<string>(5);

        // start publisher
        var baristaTask = _barista.StartMakingCoffeeAsync(channel.Writer, cts.Token);

        // coffee shop opens, customers waiting outside can come in all at once
        var customerTasks = _customers.Select(c => c.ConsumeCoffeeAsync(channel.Reader, cts.Token)).ToArray();

        // finish
        await baristaTask;

        // finish coffees in flight
        await Task.WhenAll(customerTasks);

        Console.WriteLine("Coffee shop is closed. No more coffee.");
    }
}
