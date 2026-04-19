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
        public void BreakfastSynchronous(int eggsCount, int hashBrownsCount, int toastSlices)
        {
            Coffee cup = coffeeService.PourCoffee();
            Console.WriteLine("coffee is ready");
            Console.WriteLine();

            Egg eggs = eggService.FryEggs(eggsCount);
            Console.WriteLine("eggs are ready");
            Console.WriteLine();

            HashBrown hashBrown = hashBrownService.FryHashBrowns(hashBrownsCount);
            Console.WriteLine("hash browns are ready");
            Console.WriteLine();

            Toast toast = toastService.ToastBread(toastSlices);
            toastService.ApplyButter(toast);
            toastService.ApplyJam(toast);
            Console.WriteLine("toast is ready");
            Console.WriteLine();

            Juice oj = juiceService.PourOJ();
            Console.WriteLine("oj is ready");
            Console.WriteLine("Breakfast is ready!");
            Console.WriteLine();
        }

        public async Task BreakfastAwait(int eggsCount, int hashBrownsCount, int toastSlices)
        {
            Coffee cup = await coffeeService.PourCoffeeAsync();
            Console.WriteLine("coffee is ready");
            Console.WriteLine();

            Egg eggs = await  eggService.FryEggsAsync(eggsCount);
            Console.WriteLine("eggs are ready");
            Console.WriteLine();

            HashBrown hashBrown = await hashBrownService.FryHashBrownsAsync(hashBrownsCount);
            Console.WriteLine("hash browns are ready");
            Console.WriteLine();

            Toast toast = await toastService.ToastBreadAsync(toastSlices);
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

        public async Task BreakfastConcurrent(int eggsCount, int hashBrownsCount, int toastSlices)
        {
            var pourCoffeeTask = coffeeService.PourCoffeeAsync();
            var fryEggsTask = eggService.FryEggsAsync(eggsCount);
            var fryHashBrownsTask = hashBrownService.FryHashBrownsAsync(hashBrownsCount);
            var makeToastWithButterAndJamTask = toastService.MakeToastWithButterAndJamAsync(toastSlices);
            var pourOJTask = juiceService.PourOJAsync();

            Coffee cup = await pourCoffeeTask;
            Console.WriteLine("coffee is ready");
            Console.WriteLine();

            Egg eggs = await fryEggsTask;
            Console.WriteLine("eggs are ready");
            Console.WriteLine();

            HashBrown hashBrown = await fryHashBrownsTask;
            Console.WriteLine("hash browns are ready");
            Console.WriteLine();

            Toast toast = await makeToastWithButterAndJamTask;
            Console.WriteLine("toast is ready");
            Console.WriteLine();

            Juice oj = await pourOJTask;
            Console.WriteLine("oj is ready");
            Console.WriteLine();

            Console.WriteLine("Breakfast is ready!");
            Console.WriteLine();
        }

        public async Task BreakfastWhenAll(int eggsCount, int hashBrownsCount, int toastSlices)
        {
            var pourCoffeeTask = coffeeService.PourCoffeeAsync();
            var fryEggsTask = eggService.FryEggsAsync(eggsCount);
            var fryHashBrownsTask = hashBrownService.FryHashBrownsAsync(hashBrownsCount);
            var makeToastWithButterAndJamTask = toastService.MakeToastWithButterAndJamAsync(toastSlices);
            var pourOJTask = juiceService.PourOJAsync();

            await Task.WhenAll(pourCoffeeTask, fryEggsTask, fryHashBrownsTask, makeToastWithButterAndJamTask, pourOJTask);

            Console.WriteLine("coffee is ready");
            Console.WriteLine();

            Console.WriteLine("eggs are ready");
            Console.WriteLine();

            Console.WriteLine("hash browns are ready");
            Console.WriteLine();

            Console.WriteLine("toast is ready");
            Console.WriteLine();

            Console.WriteLine("oj is ready");
            Console.WriteLine();

            Console.WriteLine("Breakfast is ready!");
            Console.WriteLine();
        }

        public async Task BreakfastWhenAny(int eggsCount, int hashBrownsCount, int toastSlices)
        {
            var pourCoffeeTask = coffeeService.PourCoffeeAsync();
            var fryEggsTask = eggService.FryEggsAsync(eggsCount);
            var fryHashBrownsTask = hashBrownService.FryHashBrownsAsync(hashBrownsCount);
            var makeToastWithButterAndJamTask = toastService.MakeToastWithButterAndJamAsync(toastSlices);
            var pourOJTask = juiceService.PourOJAsync();

            var breakfastTasks = new List<Task> { pourCoffeeTask, fryEggsTask, fryHashBrownsTask, makeToastWithButterAndJamTask, pourOJTask };

            await Task.WhenAll(pourCoffeeTask, fryEggsTask, fryHashBrownsTask, makeToastWithButterAndJamTask, pourOJTask);
            while (breakfastTasks.Count > 0)
            {
                Task finishedTask = await Task.WhenAny(breakfastTasks);
                if (finishedTask == pourCoffeeTask)
                {
                    Console.WriteLine("coffee is ready");
                    Console.WriteLine();
                }
                else if (finishedTask == fryEggsTask)
                {
                    Console.WriteLine("eggs are ready");
                    Console.WriteLine();
                }
                else if (finishedTask == fryHashBrownsTask)
                {
                    Console.WriteLine("hash browns are ready");
                    Console.WriteLine();
                }
                else if (finishedTask == makeToastWithButterAndJamTask)
                {
                    Console.WriteLine("toast is ready");
                    Console.WriteLine();
                }
                else if (finishedTask == pourOJTask)
                {
                    Console.WriteLine("oj is ready");
                    Console.WriteLine();
                }

                await finishedTask;
                breakfastTasks.Remove(finishedTask);
            }

            Console.WriteLine("Breakfast is ready!");
            Console.WriteLine();
        }
    }
}
