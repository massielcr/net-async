using System.Collections.Concurrent;

namespace SystemCollectionsConcurrent
{
    internal static class DSConcurrentDictionary
    {
        internal static async Task Run(int itemCounts, int concurrencyLevel, int capacity)
        {
            Console.WriteLine($"1. Create empty ConcurrentDictionary<int,int> instance with concurrencyLevel {concurrencyLevel} and capacity {capacity}");
            ConcurrentDictionary<int, int> cd = new(concurrencyLevel, capacity);

            Console.WriteLine($"2. Add {itemCounts} items into the ConcurrentDictionary<int,int> instance");

            int taskCounter = 0;
            int itemsPerTask = (int)Math.Ceiling((double)itemCounts / concurrencyLevel);

            Action<int> processor = (taskIndex) =>
            {
                int threadId = Environment.CurrentManagedThreadId;
                int? taskId = Task.CurrentId;

                int start = taskIndex * itemsPerTask;
                int end = Math.Min(start + itemsPerTask, itemCounts);

                for (int i = start; i < end; i++)
                {
                    cd[i] = i * i;
                    Console.WriteLine($"[Worker] Thread {threadId} TaskId {taskId}| item: {i}");
                }

                Interlocked.Increment(ref taskCounter);
            };

            var tasks = Enumerable.Range(0, concurrencyLevel).Select(i => Task.Run(() => processor(i)));

            await Task.WhenAll(tasks);

            Console.WriteLine($"Tasks Count: {taskCounter}");
            Console.WriteLine("The square of 15 is {0} (should be {1})", cd[15], 15 * 15);
        }

        internal static async Task RunAddOrUpdate(int itemCounts)
        {
            Console.WriteLine($"1. Create empty ConcurrentDictionary<int,int> instance");
            ConcurrentDictionary<int, int> cd = new();

            Console.WriteLine($"2. Parallel to Add value {itemCounts} times");
            Parallel.For(0, itemCounts, i =>
            {
                // Initial call will set cd[1] = 1.
                // Ensuing calls will set cd[1] = cd[1] + 1
                cd.AddOrUpdate(1, 1, (key, oldValue) => oldValue + 1);
            });

            Console.WriteLine($"3. TryGetValue() of 2");
            int value = -1;
            cd.TryGetValue(2, out value);
            Console.WriteLine($"After initial TryGetValue, cd[2] = {value} - default(int) -");

            Console.WriteLine($"4. After GetOrAdd() value when there is no key");
            // Should return 100, as key 2 is not yet in the dictionary
            value = cd.GetOrAdd(2, (key) => 100);
            Console.WriteLine($"After initial GetOrAdd, cd[2] = {value} - should be 100 -");

            Console.WriteLine($"5. After GetOrAdd() value when there is key");
            // Should return 100, as key 2 is already set to that value
            value = cd.GetOrAdd(2, 500);
            Console.WriteLine($"After second GetOrAdd, cd[2] = {value} - should be 100 -");
        }
    }
}
