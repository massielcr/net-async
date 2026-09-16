using System.Collections.Concurrent;

namespace SystemCollectionsConcurrent
{
    internal static class DSConcurrentQueue
    {
        internal static Random random = new();

        internal static async Task Run(int itemsCount, int expectedValue)
        {
            Console.WriteLine($"1. Create empty ConcurrentQueue<int> instance");

            ConcurrentQueue<int> queue = new();

            int result;

            Console.WriteLine($"2. Peek at the first element of the empty ConcurrentQueue<int> instance");

            if (!queue.TryPeek(out result))
            {
                Console.WriteLine("CQ: TryPeek failed");
            }
            else
            {
                Console.WriteLine("CQ: TryPeek result got {0}", result);
            }

            Console.WriteLine($"3. Enqueue {itemsCount} items synchronously into the ConcurrentQueue<int> instance");

            for (int i = 0; i < itemsCount; i++)
            {
                queue.Enqueue(i);
            }


            Console.WriteLine($"4. Peek at the first element of the ConcurrentQueue<int> instance");

            if (!queue.TryPeek(out result))
            {
                Console.WriteLine("CQ: TryPeek failed when it should have succeeded");
            }
            else
            {
                Console.WriteLine("CQ: TryPeek result got {0}", result);
            }

            Console.WriteLine($"5. Dequeue items from the ConcurrentQueue<int> instance by 4 workers in parallel");

            int outerSum = 0;
            // An action to consume the ConcurrentQueue.
            Action action = () =>
            {
                int threadId = Environment.CurrentManagedThreadId;

                Console.WriteLine($"[Worker {Task.CurrentId}] Thread {threadId} started");

                int localSum = 0;
                int localValue;
                while (queue.TryDequeue(out localValue))
                {
                    localSum += localValue;
                }
                int exactSumAfterMe =  Interlocked.Add(ref outerSum, localSum);

                Console.WriteLine($"[Worker {Task.CurrentId}] Thread {threadId} finished | exactSumAfterMe:{exactSumAfterMe}");
            };

            // Start 4 concurrent consuming actions.
            Parallel.Invoke(action, action, action, action);

            Console.WriteLine("outerSum = {0}, should be {1}", outerSum, expectedValue);
        }

        internal static async Task RunProducerConsumerDeadlock(int itemsCount, bool deadlock = true)
        {
            Console.WriteLine($"1. Create empty ConcurrentQueue<int> instance");
            ConcurrentQueue<int> queue = new();

            int consumerIsReadyForNextItem = 1;
            int producerHasEnqueuedAnItem = 0;

            // 1. THE PRODUCER (Only adds data)
            Task producer = Task.Run(() =>
            {
                int threadId = Environment.CurrentManagedThreadId;

                for (int i = 0; i < itemsCount; i++)
                {
                    // DEADLOCK STEP 1: The producer refuses to enqueue the next item 
                    // until the consumer signals that it is ready.
                    Console.WriteLine($"[Producer Worker] Thread {threadId} | producerHasEnqueuedAnItem: {producerHasEnqueuedAnItem} consumerIsReadyForNextItem: {consumerIsReadyForNextItem}");

                    while (Volatile.Read(ref consumerIsReadyForNextItem) == 0)
                    {
                        Thread.Sleep(1); // Spinning forever
                    }
                    // Reset the ready signal for the next loop iteration
                    Interlocked.Exchange(ref consumerIsReadyForNextItem, 0);


                    queue.Enqueue(i);
                    Console.WriteLine($"[Producer Worker] Thread {threadId} | Produce: {i}");

                    // Signal to the consumer that an item is ready
                    Interlocked.Exchange(ref producerHasEnqueuedAnItem, 1);

                    Thread.Sleep(10);
                }
            });

            // 2. THE CONSUMER (Only removes data)
            Task consumer = Task.Run(() =>
            {
                int threadId = Environment.CurrentManagedThreadId;

                if (deadlock)
                {
                    // DEADLOCK STEP 2: The consumer refuses to process anything 
                    // until the producer signals that an item has been enqueued.
                    Console.WriteLine($"[Consumer Worker] Thread {threadId} | producerHasEnqueuedAnItem: {producerHasEnqueuedAnItem} consumerIsReadyForNextItem: {consumerIsReadyForNextItem}");

                    while (Volatile.Read(ref producerHasEnqueuedAnItem) == 0)
                    {
                        Thread.Sleep(1); // Spinning forever
                    }
                    // Reset the enqueue signal for the next item
                    Interlocked.Exchange(ref producerHasEnqueuedAnItem, 0);

                    if (queue.TryDequeue(out int result))
                    {
                        Console.WriteLine($"[Consumer Worker] Thread {threadId}  | Consumed: {result}");
                    }

                    // Signal back to the producer that it can send the next item
                    Interlocked.Exchange(ref consumerIsReadyForNextItem, 1);                    
                }
                else
                {
                    // AVOID DEADLOCK STEP 2: Consume all the items according to itemsCount, not only depending on what is on the queue,
                    // otherwise if the queue gets empty by any chance and the producerHasEnqueuedAnItem is 0 the task will be completed 
                    // and we won't have any consumer task even if the poducer queues another item
                    for (int c = 0; c < itemsCount; c++)
                    {
                        Console.WriteLine($"[Consumer Worker] Thread {threadId} | producerHasEnqueuedAnItem: {producerHasEnqueuedAnItem} consumerIsReadyForNextItem: {consumerIsReadyForNextItem}");

                        while (Volatile.Read(ref producerHasEnqueuedAnItem) == 0)
                        {
                            Thread.Sleep(1); // Spinning forever
                        }
                        // Reset the enqueue signal for the next item
                        Interlocked.Exchange(ref producerHasEnqueuedAnItem, 0);

                        if (queue.TryDequeue(out int result))
                        {
                            Console.WriteLine($"[Consumer Worker] Thread {threadId}  | Consumed: {result}");
                        }

                        // Signal back to the producer that it can send the next item
                        Interlocked.Exchange(ref consumerIsReadyForNextItem, 1);
                    }
                }                
            });

            Console.WriteLine($"2. Run Producer and Consumer tasks");

            await Task.WhenAll(producer, consumer);
        }

        internal static async Task RunResourceDependencyLivelock(int itemsCount, int workersCount)
        {
            Console.WriteLine($"1. Create empty ConcurrentQueue<int> queueA and queueB instances");

            ConcurrentQueue<int> queueA = new();
            ConcurrentQueue<int> queueB = new();

            // Worker 1: Drains Queue A, but stalls if Queue B has items
            Action worker1Action = () =>
            {
                int result;
                // Loop runs as long as there is work in Queue A
                while (queueA.TryDequeue(out result))
                {
                    // DEADLOCK CONDITION: Worker 1 refuses to proceed until 
                    // Queue B is completely empty.
                    while (!queueB.IsEmpty)
                    {
                        Thread.Sleep(1); // Spin-waiting indefinitely
                    }
                }
                Console.WriteLine("Worker 1 finished"); // Never reached
            };

            // Worker 2: Drains Queue B, but stalls if Queue A has items
            Action worker2Action = () =>
            {
                int result;
                // Loop runs as long as there is work in Queue B
                while (queueB.TryDequeue(out result))
                {
                    // DEADLOCK CONDITION: Worker 2 refuses to proceed until 
                    // Queue A is completely empty.
                    while (!queueA.IsEmpty)
                    {
                        Thread.Sleep(1); // Spin-waiting indefinitely
                    }
                }
                Console.WriteLine("Worker 2 finished"); // Never reached
            };

            // Seed each queue with initial items so the loops start running
            for (int i = 0; i < itemsCount; i++)
            {
                queueA.Enqueue(i);
                queueB.Enqueue(i);
            }

            // Start both workers concurrently
            Task[] resultTasks =
            [
                Task.Run(worker1Action),
                Task.Run(worker2Action)
            ];

            await Task.WhenAll(resultTasks);
        }        
    }
}
