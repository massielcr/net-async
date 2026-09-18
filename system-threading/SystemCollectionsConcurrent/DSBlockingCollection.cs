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
                        while (true) // =>  busy waitting forever. 
                        {
                            Console.WriteLine($"[Consumer worker {Task.CurrentId}] Thread {threadId} | Consumed {bc.Take()}"); // => It throws an exception when CompleteAdding() has been called
                        }
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

                    while (bc.TryTake(out localItem, Timeout.Infinite)) // =>  tells the thread to wait forever if the collection is empty. The loop stops and returns false only when CompleteAdding() has been called on the collection and it is completely empty
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

        public static async Task RunTryTakeFromAny(int itemCounts, int upperbound)
        {
            Console.WriteLine($"1. Create a BlockingCollection<int>[] instance with 2 BlockingCollection<int> with upperboud {upperbound}");
            BlockingCollection<int>[] bcs = [
                new BlockingCollection<int>(upperbound),
                new BlockingCollection<int>(upperbound)
            ];

            int producerCounter = 0;
            Console.WriteLine($"2. TryAddToAny() {itemCounts} items");
            Action<int> producer = (i) =>
            {
                int threadId = Environment.CurrentManagedThreadId;
                int? taskId = Task.CurrentId;

                int resultCollection = -1;
                while ((resultCollection = BlockingCollection<int>.TryAddToAny(bcs, i)) == -1)
                {
                    Thread.Sleep(1);
                }

                Interlocked.Increment(ref producerCounter);
                Console.WriteLine($"[Producer Worker {Task.CurrentId}] Thread {threadId} | Produced {i} into resultCollection:{resultCollection}");
            };

            Action producerComplete = () =>
            {
                int threadId = Environment.CurrentManagedThreadId;
                int? taskId = Task.CurrentId;

                foreach (var bc in bcs)
                {
                    bc.CompleteAdding();
                }

                Console.WriteLine($"[Producer Completion Worker {Task.CurrentId}] Thread {threadId} | Sent CompleteAdding()");
            };

            var producerTasks = Enumerable.Range(0, itemCounts).Select(i => 
                {
                    int itemToProduce = i;
                    return Task.Run(() => producer(itemToProduce));
                })
                .ToArray();
            var producerCompleteTask = Task.WhenAll(producerTasks).ContinueWith(_ => producerComplete());


            Console.WriteLine($"3. TryTakeFromAny() {itemCounts} items");
            int consumerCounter = 0;
            Action consumer = () =>
            {
                int threadId = Environment.CurrentManagedThreadId;
                int? taskId = Task.CurrentId;

                try
                {
                    // TakeFromAny will block efficiently until an item is ready.
                    // It automatically throws an ArgumentException when all collections are marked complete AND empty.
                    while (true)
                    {
                        int resultCollection = BlockingCollection<int>.TakeFromAny(bcs, out int item);

                        Interlocked.Increment(ref consumerCounter);
                        Console.WriteLine($"[Consumer Worker {Task.CurrentId}] Thread {threadId} | Consumed {item} resultCollection:{resultCollection}");
                    }
                }
                catch (ArgumentException)
                {
                    // This exception is explicitly thrown by TakeFromAny when all collections in the array are marked as completed.
                    Console.WriteLine($"[Consumer Worker {Task.CurrentId}] Thread {threadId} | All collections completed. Exiting consumer safely.");
                }
            };

            var consumerTask = Task.Run(consumer);

            await Task.WhenAll(producerCompleteTask, consumerTask);

            Console.WriteLine($"itemCounts: {itemCounts} | producerCounter: {producerCounter} | consumerCounter: {consumerCounter}");
        }

        public static async Task RunEnumerable(int itemCounts, int upperbound)
        {
            using (BlockingCollection<int> bc = new BlockingCollection<int>(upperbound))
            {
                Task producerTask = Task.Run(() =>
                {
                    int threadId = Environment.CurrentManagedThreadId;
                    int? taskId = Task.CurrentId;

                    for(int i = 0; i < itemCounts; i++)
                    {
                        bc.Add(i);
                        Console.WriteLine($"[Producer Worker {Task.CurrentId}] Thread {threadId} | Produced {i}");
                    }

                    bc.CompleteAdding();
                });

                foreach(int item in bc.GetConsumingEnumerable())
                {
                    Console.WriteLine($"[Consumer Worker] | Consumed {item}");
                }

                await producerTask;
            }
        }
    }
}

