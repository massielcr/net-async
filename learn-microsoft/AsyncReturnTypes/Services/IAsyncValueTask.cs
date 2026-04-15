namespace AsyncReturnTypes.Services
{
    public interface IAsyncValueTask
    {
        ValueTask<int> RollAsync();
    }
}
