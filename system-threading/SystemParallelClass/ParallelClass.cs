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
    }
}
