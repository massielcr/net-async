namespace AsyncProgrammingScenarios.Services
{
    public interface IDotNetAPIService
    {
        Task<int> GetDotNetCountAsync(string URL);
    }
}
