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
        public void Synchronous()
        {
            Coffee cup = coffeeService.PourCoffee();
            Console.WriteLine("coffee is ready");

            Egg eggs = eggService.FryEggs(2);
            Console.WriteLine("eggs are ready");

            HashBrown hashBrown = hashBrownService.FryHashBrowns(3);
            Console.WriteLine("hash browns are ready");

            Toast toast = toastService.ToastBread(2);
            toastService.ApplyButter(toast);
            toastService.ApplyJam(toast);
            Console.WriteLine("toast is ready");

            Juice oj = juiceService.PourOJ();
            Console.WriteLine("oj is ready");
            Console.WriteLine("Breakfast is ready!");
        }
    }
}
