using System.Collections.Concurrent;

namespace SystemCollectionsConcurrent
{
    internal static class DSConcurrentBag
    {
        internal static async Task Run(int itemsCount)
        {
            Console.WriteLine($"1. Create empty ConcurrentBag<int> instance");
            ConcurrentBag<int> cb = [];

            Console.WriteLine($"2. Add {itemsCount} items to the bag by {itemsCount} workers, 1 task per item");
            List<Task> bagAddTasks = [];
            for (int i = 0; i < itemsCount; i++)
            {
                var numberToAdd = i;
                bagAddTasks.Add(Task.Run(() => cb.Add(numberToAdd)));

                Console.WriteLine($"Added {numberToAdd} item into the bag");
            }

            // Wait for all tasks to complete
            Task.WaitAll(bagAddTasks.ToArray());

            Console.WriteLine($"3. Consume the items in the bag while the bag is not empty by {itemsCount} workers, 1 task per item");
            List<Task> bagConsumeTasks = [];
            int itemsInBag = 0;
            while (!cb.IsEmpty)
            {
                bagConsumeTasks.Add(Task.Run(() =>
                {
                    int threadId = Environment.CurrentManagedThreadId;

                    int item;
                    if (cb.TryTake(out item))
                    {                        
                        Interlocked.Increment(ref itemsInBag);
                        Console.WriteLine($"[Worker] Thread {threadId:2} | Took {item} - itemsInBag: {itemsInBag}");
                    }
                }));
            }
            Task.WaitAll(bagConsumeTasks.ToArray());

            Console.WriteLine($"There were {itemsInBag} items in the bag");

            // Checks the bag for an item
            // The bag should be empty and this should not print anything
            int unexpectedItem;
            if (cb.TryPeek(out unexpectedItem))
                Console.WriteLine("Found an item in the bag when it should be empty");
        }
    }
}
