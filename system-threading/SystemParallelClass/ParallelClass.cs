namespace SystemParallelClass
{
    internal static class ParallelClass
    {
        public static void RunParallel(int upperBound)
        {
            Console.WriteLine("1. Implement the Action");
            Action<int> action = (n) =>
            {
                Console.WriteLine($"Current {n}");
            };

            Console.WriteLine($"2. Implement Parallel passing Action<int> {upperBound} times");
            Parallel.For(0, upperBound, action);

            Console.WriteLine($"3. Implement Parallel passing a delegate {upperBound} times");
            Parallel.For(0, upperBound, delegate (int i)
            {
                action(i);
            });

            Console.WriteLine($"4. Implement Parallel passing a lambda {upperBound} times");
            Parallel.For(0, upperBound, i =>
            {
                action(i);
            });
        }

        public static void RunParallelForBreak(int breakIndexUpperBound, int lowerBound, int upperBound, int delayUpperBound)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine($"1. Generate breakIndex");
            int breakIndex = Random.Shared.Next(1, breakIndexUpperBound);

            Console.WriteLine($"2. Start the Parallel.For from {lowerBound} to {upperBound}");
            ParallelOptions options = new() { MaxDegreeOfParallelism = 2 };
            var result = Parallel.For(lowerBound, upperBound, options, (i, state) =>
            {
                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} i: {i}");

                if (state.ShouldExitCurrentIteration && i > state.LowestBreakIteration)
                {
                    return;
                }

                int delay = Random.Shared.Next(200, delayUpperBound);

                Thread.Sleep(delay);                

                if (i == breakIndex)
                {
                    Console.WriteLine($"Break in iteration {i}");
                    state.Break();
                }
            });

            Console.WriteLine("3. Display the Results");
            if (result.IsCompleted)
            {
                Console.WriteLine("Completed Iteration");
            }
            else if (result.LowestBreakIteration is not null)
            {
                Console.WriteLine("Broken Iteration");
            }
            else
            {
                Console.WriteLine("Stopped Iteration");
            }
        }

        public static void RunParallelForStop(int breakIndexUpperBound, int lowerBound, int upperBound, int delayUpperBound)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine("1. Generate stopIndex");
            int stopIndex = Random.Shared.Next(1, breakIndexUpperBound);

            Console.WriteLine($"Start an iteration from {lowerBound} to {upperBound}");
            ParallelOptions options = new() { MaxDegreeOfParallelism = 2 };
            var result = Parallel.For(lowerBound, upperBound, options, (i, state) =>
            {
                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} i: {i}");

                int delay = Random.Shared.Next(200, delayUpperBound);

                Thread.Sleep(delay);

                if (state.IsStopped)
                {
                    return;
                }

                if (i == stopIndex)
                {
                    Console.WriteLine($"Stopped in iteration {i}");
                    state.Stop();
                }
            });

            Console.WriteLine("3. Display the Results");
            if (result.IsCompleted)
            {
                Console.WriteLine("Completed Iteration");
            }
            else if (result.LowestBreakIteration is not null)
            {
                Console.WriteLine("Broken Iteration");
            }
            else
            {
                Console.WriteLine("Stopped Iteration");
            }
        }
    }
}
