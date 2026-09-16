using SystemCollectionsConcurrent;

class Program
{
    static async Task Main()
    {
        while (true)
        {
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("[1] - ConcurrentStack");
            Console.WriteLine("[2] - ConcurrentStack - Range");
            Console.WriteLine("[3] - ConcurrentStack - Deadlock");
            Console.WriteLine("[4] - ConcurrentQueue");
            Console.WriteLine("[5] - ConcurrentQueue - Deadlock");
            Console.WriteLine("[6] - ConcurrentQueue - Deadlock (Fixed)");

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
                    await DSConcurrentStack.RunDeadlock(900, 5);
                    goto default;
                case "4":
                    Console.WriteLine("CONCURRENTQUEUE:");
                    await DSConcurrentQueue.Run(10000, 49995000);
                    goto default;
                case "5":
                    Console.WriteLine("CONCURRENTQUEUE - Deadlock:");
                    await DSConcurrentQueue.RunProducerConsumerDeadlock(100);
                    goto default;
                case "6":
                    Console.WriteLine("CONCURRENTQUEUE - Deadlock (Fixed):");
                    await DSConcurrentQueue.RunProducerConsumerDeadlock(100, false);
                    goto default;
                default:
                    Console.WriteLine();
                    break;
            }
        }
    }    
}






