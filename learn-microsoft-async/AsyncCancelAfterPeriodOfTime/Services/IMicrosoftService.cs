namespace AsyncCancelAfterPeriodOfTime.Services
{
    public interface IMicrosoftService
    {
        Task SumPageSizesAsync(CancellationToken cancellationToken);
    }
}
