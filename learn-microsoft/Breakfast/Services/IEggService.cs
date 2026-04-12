using Breakfast.CL.Models;

namespace Breakfast.CL.Services
{
    public interface IEggService
    {
        Egg FryEggs(int count);

        Task<Egg> FryEggsAsync(int count);
    }
}
