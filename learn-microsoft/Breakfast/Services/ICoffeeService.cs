using Breakfast.CL.Models;

namespace Breakfast.CL.Services
{
    public interface ICoffeeService
    {
        Coffee PourCoffee();

        Task<Coffee> PourCoffeeAsync();
    }
}
