namespace AsyncReturnTypes.Services
{
    public class AsyncValueTask : IAsyncValueTask
    {
        static readonly Random s_rnd = new();

        private int? _lastRoll;

        public async ValueTask<int> RollAsync()
        {
            if (_lastRoll.HasValue)
            {
                var roll = _lastRoll.Value;
                _lastRoll = null; // Clear it for next time
                return roll;
            }

            await Task.Delay(500);

            int diceRoll = s_rnd.Next(1, 7);
            return diceRoll;
        }
    }
}
