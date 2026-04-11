using Breakfast.CL.Models;

namespace Breakfast.CL.Services
{
    public interface IHashBrownService
    {
        HashBrown FryHashBrowns(int patties);

        Task<HashBrown> FryHashBrownsAsync(int patties);
    }
}