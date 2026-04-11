using Breakfast.CL.Models;
using Breakfast.CL.Services;

namespace Breakfast
{
    public class BreakfastMaker(ICoffeeService coffeeService, 
                                IEggService eggService, 
                                IHashBrownService hashBrownService, 
                                IToastService toastService,
                                IJuiceService juiceService)
    {
        public void BreakfastSynchronous()
        {
            Coffee cup = coffeeService.PourCoffee();
            Console.WriteLine("coffee is ready");
            Console.WriteLine();

            Egg eggs = eggService.FryEggs(2);
            Console.WriteLine("eggs are ready");
            Console.WriteLine();

            HashBrown hashBrown = hashBrownService.FryHashBrowns(3);
            Console.WriteLine("hash browns are ready");
            Console.WriteLine();

            Toast toast = toastService.ToastBread(2);
            toastService.ApplyButter(toast);
            toastService.ApplyJam(toast);
            Console.WriteLine("toast is ready");
            Console.WriteLine();

            Juice oj = juiceService.PourOJ();
            Console.WriteLine("oj is ready");
            Console.WriteLine("Breakfast is ready!");
            Console.WriteLine();
        }

        public async Task BreakfastAwait()
        {
            Coffee cup = await coffeeService.PourCoffeeAsync();
            Console.WriteLine("coffee is ready");
            Console.WriteLine();

            Egg eggs = await  eggService.FryEggsAsync(2);
            Console.WriteLine("eggs are ready");
            Console.WriteLine();

            HashBrown hashBrown = await hashBrownService.FryHashBrownsAsync(3);
            Console.WriteLine("hash browns are ready");
            Console.WriteLine();

            Toast toast = await toastService.ToastBreadAsync(2);
            toastService.ApplyButter(toast);
            toastService.ApplyJam(toast);
            Console.WriteLine("toast is ready");
            Console.WriteLine();

            Juice oj = await juiceService.PourOJAsync();
            Console.WriteLine("oj is ready");
            Console.WriteLine();

            Console.WriteLine("Breakfast is ready!");
            Console.WriteLine();
        }
    }
}
