using Breakfast.CL.Models;

namespace Breakfast.CL.Services
{
    public interface IJuiceService
    {
        Juice PourOJ();

        Task<Juice> PourOJAsync();
    }
}
