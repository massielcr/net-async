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
    }
}
