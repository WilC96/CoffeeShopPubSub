using System.Threading.Channels;

namespace CoffeeShopPubSub.Services;

public sealed class Customer
{
    private readonly string _name;
    private readonly int _drinkingSpeedMs;

    public Customer(string name, DrinkingSpeed drinkingSpeed)
    {
        _name = name;
        _drinkingSpeedMs = (int)drinkingSpeed;
    }

    public async Task ConsumeCoffeeAsync(ChannelReader<string> reader, CancellationToken cancellationToken)
    {
        await foreach (var coffeeOrder in reader.ReadAllAsync(cancellationToken))
        {
            Console.WriteLine($"{_name} is picking up {coffeeOrder}...");

            await Task.Delay(_drinkingSpeedMs);
            Console.WriteLine($"{_name} finished drinking {coffeeOrder}.");
        }

        Console.WriteLine($"{_name} has no more coffee to consume. Going home.");
    }
}

public enum DrinkingSpeed
{
    CaffeineHigh = 1000,
    Leisurely = 2000,
    PrefersTea = 3000
}