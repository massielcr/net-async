using AsyncProgrammingScenarios.Models;

namespace AsyncProgrammingScenarios.Services
{
    public interface IUserDBService
    {
        Task<IEnumerable<User>> GetUsersAsync(IEnumerable<int> userIds);
        Task<User[]> GetUsersByLINQAsync(IEnumerable<int> userIds);
        Task<User> GetUserAsync(int userId);
    }
}
