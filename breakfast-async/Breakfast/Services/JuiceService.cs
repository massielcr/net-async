using Breakfast.CL.Models;

namespace Breakfast.CL.Services
{
    public class JuiceService : IJuiceService
    {
        public Juice PourOJ()
        {
            Console.WriteLine("Pouring orange juice");
            return new Juice();
        }

        public Task<Juice> PourOJAsync()
        {
            throw new NotImplementedException();
        }
    }
}
