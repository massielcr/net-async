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
                bagAddTasks.Add(Task.Run(() => 
                {
                    cb.Add(numberToAdd);

                    Console.WriteLine($"[Producer] Added {numberToAdd} item into the bag");
                }));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(bagAddTasks);

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
                        Console.WriteLine($"[Consumer] Thread {threadId} | Took {item} - itemsInBag: {Volatile.Read(ref itemsInBag)}");
                    }
                }));
            }
            await Task.WhenAll(bagConsumeTasks);

            Console.WriteLine($"There were {itemsInBag} items in the bag");

            // Checks the bag for an item
            // The bag should be empty and this should not print anything
            int unexpectedItem;
            if (cb.TryPeek(out unexpectedItem))
                Console.WriteLine("Found an item in the bag when it should be empty");
        }
    }
}
