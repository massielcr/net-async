using System.Collections.Concurrent;

namespace SystemCollectionsConcurrent
{
    internal static class DSBlockingCollection
    {
        public static async Task RunTake(int itemCounts, int upperbound)
        {
            Console.WriteLine($"1. Create a BlockingCollection<int> instance with upperboud {upperbound}");
            using (BlockingCollection<int> bc = new(upperbound))
            {
                Console.WriteLine($"2. Added task to produce {itemCounts} items into the BlockingCollection<int> instance and send CompleteAdding() signal");

                Task producer = Task.Run(() =>
                {
                    int threadId = Environment.CurrentManagedThreadId;

                    for (int i = 0; i < itemCounts; i++)
                    {
                        bc.Add(i);
                        Console.WriteLine($"[Producer Worker {Task.CurrentId}] Thread {threadId} | Produced {i}");
                    }
                    bc.CompleteAdding();

                    Console.WriteLine($"[Producer Worker {Task.CurrentId}] Thread {threadId} | Send CompleteAdding() signal");
                });

                Console.WriteLine($"3. Added Task to Consume {itemCounts} items from the BlockingCollection<int> instance");

                Task consumer = Task.Run(() =>
                {
                    int threadId = Environment.CurrentManagedThreadId;

                    try
                    {
                        // Consume the BlockingCollection - bc.Take() relies on CompleteAdding() to know how to behave when the collection is empty
                        while (true) Console.WriteLine($"[Consumer worker {Task.CurrentId}] Thread {threadId} | Consumed {bc.Take()}");
                    }
                    catch (InvalidOperationException)
                    {
                        // An InvalidOperationException means that Take() was called on a completed collection
                        Console.WriteLine("That's All! InvalidOperationException was thrown when Take failed");
                    }
                });

                Console.WriteLine($"4. Run Producer and Consumer workers");

                await Task.WhenAll(producer, consumer);
            }
        }

        public static async Task RunTryTake(int itemCounts, int upperbound)
        {
            Console.WriteLine($"1. Create a BlockingCollection<int> instance with upperboud {upperbound}");
            using (BlockingCollection<int> bc = new(upperbound))
            {
                Console.WriteLine($"2. Add {itemCounts} items to BlockingCollection<int> instance and send signal CompleteAdding()");             
                Action producer = () =>
                {
                    int threadId = Environment.CurrentManagedThreadId;

                    for (int i = 0; i < itemCounts; i++)
                    {
                        bc.Add(i); // This can safely block now, because consumers will start in parallel
                        Console.WriteLine($"[Producer Worker {Task.CurrentId}] Thread {threadId} | Produced {i}");
                    }
                    bc.CompleteAdding();
                };

                int outerSum = 0;

                // Delegate for consuming the BlockingCollection and adding up all items
                Action consumer = () =>
                {
                    int threadId = Environment.CurrentManagedThreadId;

                    int localItem;
                    int localSum = 0;

                    while (bc.TryTake(out localItem, Timeout.Infinite))
                    {
                        localSum += localItem;
                        Console.WriteLine($"[Consumer worker {Task.CurrentId}] Thread {threadId} | Consumed {localItem}");
                    }

                    Interlocked.Add(ref outerSum, localSum);
                };

                // Launch three parallel actions to consume the BlockingCollection
                Console.WriteLine($"3. Run 1 producer and 3 consumer workers in parallel to process the items");
                Parallel.Invoke(producer, consumer, consumer, consumer);

                Console.WriteLine("Sum[0..{0}) = {1}, should be {2}", itemCounts, outerSum, ((itemCounts * (itemCounts - 1)) / 2));
                Console.WriteLine("bc.IsCompleted = {0} (should be true)", bc.IsCompleted);
            }
        }
    }
}

