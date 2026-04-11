using Breakfast.CL.Models;

namespace Breakfast.CL.Services
{
    internal class CoffeeService : ICoffeeService
    {
        public Coffee PourCoffee()
        {
            Console.WriteLine("Pouring coffee");
            return new Coffee();
        }

        public async Task<Coffee> PourCoffeeAsync()
        {
            Console.WriteLine("Pouring coffee");
            return new Coffee();
        }
    }
}
