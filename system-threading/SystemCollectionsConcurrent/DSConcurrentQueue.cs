using System.Collections.Concurrent;

namespace SystemCollectionsConcurrent
{
    internal static class DSConcurrentQueue
    {
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

        internal static async Task RunDeadlock(int itemsCount, int workersCount)
        {
            ConcurrentQueue<int> queue = new();
        }
    }
}
