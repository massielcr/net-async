using SystemCollectionsConcurrent;

class Program
{
    static async Task Main()
    {
        while (true)
        {
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("[1]  - ConcurrentStack");
            Console.WriteLine("[2]  - ConcurrentStack - Range");
            Console.WriteLine("[3]  - ConcurrentStack - Deadlock");
            Console.WriteLine("[4]  - ConcurrentQueue");
            Console.WriteLine("[5]  - ConcurrentQueue - Deadlock");
            Console.WriteLine("[6]  - ConcurrentQueue - Deadlock (Fixed)");
            Console.WriteLine("[7]  - ConcurrentQueue - Resource Dependency Livelock");
            Console.WriteLine("[8]  - ConcurrentQueue - Resource Dependency Livelock (Fixed)");
            Console.WriteLine("[9]  - ConcurrentBag");
            Console.WriteLine("[10] - IProducerConsumerCollection<T>");
            Console.WriteLine("[11] - BlockingCollection - Take");
            Console.WriteLine("[12] - BlockingCollection - TryTake");
            Console.WriteLine("[13] - BlockingCollection - TryTakeFromAny");

            string? key = Console.ReadLine();

            switch (key)
            {
                case "1":
                    Console.WriteLine("CONCURRENTSTACK:");
                    await DSConcurrentStack.Run(900, 5);
                    goto default;
                case "2":
                    Console.WriteLine("CONCURRENTSTACK - Range:");
                    await DSConcurrentStack.RunRange(900, 5);
                    goto default;
                case "3":
                    Console.WriteLine("CONCURRENTSTACK - Deadlock:");
                    await DSConcurrentStack.RunDiningPhilosophersDeadlock(900, 5);
                    goto default;
                case "4":
                    Console.WriteLine("CONCURRENTQUEUE:");
                    await DSConcurrentQueue.Run(10000, 49995000);
                    goto default;
                case "5":
                    Console.WriteLine("CONCURRENTQUEUE - Deadlock:");
                    await DSConcurrentQueue.RunProducerConsumerDeadlock(50);
                    goto default;
                case "6":
                    Console.WriteLine("CONCURRENTQUEUE - Deadlock (Fixed):");
                    await DSConcurrentQueue.RunProducerConsumerDeadlock(50, false);
                    goto default;
                case "7":
                    Console.WriteLine("CONCURRENTQUEUE - Resource Dependency Livelock:");
                    await DSConcurrentQueue.RunResourceDependencyLivelock(50);
                    goto default;
                case "8":
                    Console.WriteLine("CONCURRENTQUEUE - Resource Dependency Livelock (Fixed):");
                    await DSConcurrentQueue.RunResourceDependencyLivelock(50, false);
                    goto default;
                case "9":
                    Console.WriteLine("CONCURRENTBAG");
                    await DSConcurrentBag.Run(50);
                    goto default;
                case "10":
                    Console.WriteLine("IPRODUCERCONSUMERCOLLECTION<T>");
                    await DSProducerConsumerCollection.Run();
                    goto default;
                case "11":
                    Console.WriteLine("BLOCKINGCOLLECTION - Take");
                    await DSBlockingCollection.RunTake(50, 5);
                    await DSBlockingCollection.RunTake(50, 50);
                    await DSBlockingCollection.RunTake(50, 60);
                    goto default;
                case "12":
                    Console.WriteLine("BLOCKINGCOLLECTION - TryTake");
                    await DSBlockingCollection.RunTryTake(50, 5);
                    await DSBlockingCollection.RunTryTake(50, 50);
                    await DSBlockingCollection.RunTryTake(50, 60);
                    goto default;
                case "13":
                    Console.WriteLine("BLOCKINGCOLLECTION - TryTakeFromAny");
                    await DSBlockingCollection.RunTryTakeFromAny(20, 5);
                    goto default;
                default:
                    Console.WriteLine();
                    break;
            }
        }
    }    
}






