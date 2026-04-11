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

        public Task<Coffee> PourCoffeeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
