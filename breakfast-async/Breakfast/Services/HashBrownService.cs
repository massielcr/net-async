using Breakfast.CL.Models;

namespace Breakfast.CL.Services
{
    public class HashBrownService : IHashBrownService
    {
        public HashBrown FryHashBrowns(int patties)
        {
            Console.WriteLine($"putting {patties} hash brown patties in the pan");
            Console.WriteLine("cooking first side of hash browns...");
            Task.Delay(3000).Wait();
            for (int patty = 0; patty < patties; patty++)
            {
                Console.WriteLine("flipping a hash brown patty");
            }
            Console.WriteLine("cooking the second side of hash browns...");
            Task.Delay(3000).Wait();
            Console.WriteLine("Put hash browns on plate");

            return new HashBrown();
        }

        public Task<HashBrown> FryHashBrownsAsync(int patties)
        {
            throw new NotImplementedException();
        }
    }
}
