using System.Collections.Concurrent;

namespace SystemCollectionsConcurrent
{
    internal static class DSConcurrentStack
    {
        static readonly object LockA = new();
        static readonly object LockB = new();

        internal static async Task Run(int itemsCount, int workersCount)
        {
            Random random = new();

            Console.WriteLine($"1. Create empty ConcurrentStack<int> instance");

            ConcurrentStack<int> stack = new();


            Console.WriteLine($"2. TryPeek() empty ConcurrentStack<int> instance");

            if (stack.TryPeek(out int emptyStackResult))
            {
                Console.WriteLine($"TryPeek() saw {emptyStackResult} on top of the stack.");
            }
            else
            {
                Console.WriteLine("Could not peek most recently added number.");
            }


            Console.WriteLine($"3. Populate ConcurrentStack<int> with {itemsCount} items sync");

            // Create an action to push items onto the stack
            Action pusher = () =>
            {
                for (int i = 0; i < itemsCount; i++)
                {
                    stack.Push(i);
                }
            };

            // Run the action once
            pusher();

            Console.WriteLine($"4. TryPeek() not empty ConcurrentStack<int> instance");

            if (stack.TryPeek(out int result))
            {
                Console.WriteLine($"TryPeek() saw {result} on top of the stack.");
            }
            else
            {
                Console.WriteLine("Could not peek most recently added number.");
            }

            Console.WriteLine($"5. Clear not empty ConcurrentStack<int> instance");
            // Empty the stack
            stack.Clear();

            Console.WriteLine($"6. Check if ConcurrentStack<int> instance is empty");

            if (stack.IsEmpty)
            {
                Console.WriteLine("Cleared the stack.");
            }

            Console.WriteLine($"7. Create {workersCount} Push and Pop workers");

            // Create an action to push and pop items
            Action pushAndPop = () =>
            {
                int threadId = Environment.CurrentManagedThreadId;

                Console.WriteLine($"Task started on Task.CurrentId: {Task.CurrentId} by Thread: {threadId} | Stack Count: {stack.Count}");

                int successfulPops = 0;
                int failedPops = 0;

                int item;
                for (int i = 0; i < itemsCount; i++)
                {
                    stack.Push(i);
                }
                    
                for (int i = 0; i < itemsCount; i++)
                {
                    if (stack.TryPop(out item))
                    {
                        successfulPops++;
                    }
                    else
                    {
                        failedPops++;
                    }
                }                    

                Console.WriteLine($"Task ended on Task.CurrentId: {Task.CurrentId} by Thread: {threadId}  | Stack Count: {stack.Count} | Successful Pops: {successfulPops} | Failed (Dry) Pops: {failedPops}");
            };

            // Spin up five concurrent tasks of the action
            var tasks = new Task[workersCount];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(pushAndPop);                
            }

            Console.WriteLine($"8.  Wait for all the tasks to finish up");

            await Task.WhenAll(tasks);

            Console.WriteLine($"9. Check if took all items from ConcurrentStack<int> instance");

            if (!stack.IsEmpty)
            {
                Console.WriteLine("Did not take all the items off the stack");
            }
            else
            {
                Console.WriteLine("It took all the items off the stack");
            }
        }

        internal static async Task RunRange(int itemsCount, int workersCount)
        {
            Console.WriteLine($"1. Create empty ConcurrentStack<int> instance");

            ConcurrentStack<int> stack = new();


            Console.WriteLine($"2. Push a range of values onto the ConcurrentStack<int> instance concurrently by {workersCount} workers");

            await Task.WhenAll(Enumerable.Range(0, workersCount).Select(i => Task.Factory.StartNew(
                (state) =>
                {
                    int threadId = Environment.CurrentManagedThreadId;

                    int index = (int)state!;

                    Console.WriteLine($"[Worker {Task.CurrentId}] Thread {threadId} | State: {index}");
                    
                    int[] array = new int[itemsCount];
                    for (int j = 0; j < itemsCount; j++)
                    {
                        array[j] = index + j;
                    }

                    Console.WriteLine($"[Worker {Task.CurrentId}] Thread {threadId} | Pushing an array of ints from {array[0]} to {array[itemsCount - 1]}");

                    stack.PushRange(array);

                }, i * itemsCount, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default)
            ).ToArray());


            Console.WriteLine($"3. Try Pop a range of values from the ConcurrentStack<int> instance concurrently by {workersCount} workers");

            int numTotalElements = workersCount * itemsCount;
            int[] resultBuffer = new int[numTotalElements];

            await Task.WhenAll(Enumerable.Range(0, workersCount).Select(i => Task.Factory.StartNew(
                obj =>
                {
                    int threadId = Environment.CurrentManagedThreadId;

                    int index = (int)obj!;

                    Console.WriteLine($"[Worker {Task.CurrentId}] Thread {threadId} | State: {index}");

                    int result = stack.TryPopRange(resultBuffer, index, itemsCount);

                    Console.WriteLine($"[Worker {Task.CurrentId}] Thread {threadId} | TryPopRange expected {itemsCount}, got {result}.");

                }, i * itemsCount, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default)
            ).ToArray());


            Console.WriteLine($"4. Verify items were inserted consecutively into the ConcurrentStack<int> instance");

            for (int i = 0; i < workersCount; i++)
            {
                int currentItemsCount = i * itemsCount;

                // Create a sequence we expect to see from the stack taking the last number of the range we inserted
                var expected = Enumerable.Range(resultBuffer[currentItemsCount + itemsCount - 1], itemsCount);

                // Take the range we inserted, reverse it, and compare to the expected sequence
                var areEqual = expected.SequenceEqual(resultBuffer.Skip(currentItemsCount).Take(itemsCount).Reverse());

                if (areEqual)
                {
                    Console.WriteLine($"Expected a range of {expected.First()} to {expected.Last()}. Got {resultBuffer[currentItemsCount + itemsCount - 1]} to {resultBuffer[i * itemsCount]}");
                }
                else
                {
                    Console.WriteLine($"Unexpected consecutive ranges.");
                }
            }
        }

        internal static async Task RunDeadlock(int itemsCount, int workersCount)
        {
            Console.WriteLine($"1. Create empty ConcurrentStack<int> instance");

            ConcurrentStack<int> stack = new();

            Console.WriteLine($"2. Create Worker 1");

            Action worker1 = () =>
            {
                int threadId = Environment.CurrentManagedThreadId;

                Console.WriteLine($"[Worker 1] Thread {threadId} attempting to lock Resource A...");

                lock (LockA)
                {
                    Console.WriteLine($"[Worker 1] Thread {threadId} successfully locked Resource A.");

                    // Tiny pause to ensure Worker 2 grabs Lock B in parallel
                    Thread.Sleep(50);

                    Console.WriteLine($"[Worker 1] Thread {threadId} attempting to lock Resource B...");
                    lock (LockB)
                    {
                        // This code will never be reached
                        for (int i = 0; i < itemsCount; i++) stack.Push(i);
                        Console.WriteLine("[Worker 1] Finished work.");
                    }
                }
            };

            Console.WriteLine($"3. Create Worker 2");

            Action worker2 = () =>
            {
                int threadId = Environment.CurrentManagedThreadId;

                Console.WriteLine($"[Worker 2] Thread {threadId} attempting to lock Resource B...");

                lock (LockB)
                {
                    Console.WriteLine($"[Worker 2] Thread {threadId} successfully locked Resource B.");

                    // Tiny pause to ensure Worker 1 grabs Lock A in parallel
                    Thread.Sleep(50);

                    Console.WriteLine($"[Worker 2] Thread {threadId} attempting to lock Resource A...");
                    lock (LockA)
                    {
                        // This code will never be reached
                        int item;
                        for (int i = 0; i < itemsCount; i++) stack.TryPop(out item);
                        Console.WriteLine("[Worker 2] Finished work.");
                    }
                }
            };

            Console.WriteLine($"4. Run both workers in parallel. Worker 1 push {itemsCount} items and Worker 2 pop {itemsCount} items");

            var tasks = new Task[2];
            tasks[0] = Task.Run(worker1);
            tasks[1] = Task.Run(worker2);

            await Task.WhenAll(tasks);

            Console.WriteLine("This line will NEVER print because of the deadlock.");
        }
    }
}
