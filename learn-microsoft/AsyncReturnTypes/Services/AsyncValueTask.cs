namespace AsyncReturnTypes.Services
{
    public class AsyncValueTask : IAsyncValueTask
    {
        static readonly Random s_rnd = new();

        public async ValueTask<int> RollAsync()
        {
            await Task.Delay(500);

            int diceRoll = s_rnd.Next(1, 7);
            return diceRoll;
        }
    }
}
