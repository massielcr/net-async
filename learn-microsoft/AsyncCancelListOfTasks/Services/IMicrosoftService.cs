namespace AsyncCancelListOfTasks.Services
{
    public interface IMicrosoftService
    {
        Task SumPageSizesAsync(CancellationToken cancellationToken);
    }
}
