using System.Collections.Concurrent;

namespace SystemCollectionsConcurrent
{
    internal static class DSBlockingCollection
    {
        public static async Task RunTake(int itemCounts)
        {
            Console.WriteLine($"1. Create a BlockingCollection<int> instance");
            using (BlockingCollection<int> bc = new())
            {
                Console.WriteLine($"2. Added task to produce {itemCounts} items into the BlockingCollection<int> instance and send CompleteAdding() signal");

                Task producer = Task.Run(() =>
                {
                    int threadId = Environment.CurrentManagedThreadId;

                    for(int i = 0; i < itemCounts; i++)
                    {
                        bc.Add(i);
                        Console.WriteLine($"[Producer Worker {Task.CurrentId}] Thread {threadId} | Produced {i}");
                    }
                    bc.CompleteAdding();

                    Console.WriteLine($"[Producer Worker {Task.CurrentId}] Thread {threadId} | Send CompleteAdding() signal");
                });

                Console.WriteLine($"3. Added Task to Consume {itemCounts} items from the BlockingCollection<int> instance after receiving the CompleteAdding() signal");

                Task consumer = Task.Run(() =>
                {
                    int threadId = Environment.CurrentManagedThreadId;

                    try
                    {
                        // Consume the BlockingCollection
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
    }
}
