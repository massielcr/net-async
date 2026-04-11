using Breakfast;
using Breakfast.CL.Services;

internal class Program
{
    private static readonly ICoffeeService _coffeeService = new CoffeeService();
    private static readonly IEggService _eggService = new EggService();
    private static readonly IHashBrownService _hashBrownService = new HashBrownService();
    private static readonly IToastService _toast_service = new ToastService();
    private static readonly IJuiceService _juiceService = new JuiceService();

    private Program() { }

    private static async Task Main()
    {
        Console.WriteLine("Hello, World!");

        var breakfastMaker = new BreakfastMaker(_coffeeService, _eggService, _hashBrownService, _toast_service, _juiceService);

        //breakfastMaker.BreakfastSynchronous();

        await breakfastMaker.BreakfastAwait(2, 3, 2);

        await breakfastMaker.BreakfastWhenAny(2, 3, 2);

        Console.ReadLine();
    }
}