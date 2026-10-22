using SystemParallelClass;

class Program
{
    static async Task Main()
    {
        while (true)
        {
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("[1]  - Parallel");
            Console.WriteLine("[2]  - Parallel - For - Break");
            Console.WriteLine("[3]  - Parallel - For - Stop");

            string? key = Console.ReadLine();

            switch (key)
            {
                case "1":
                    ParallelClass.RunParallel(3);
                    goto default;
                case "2":
                    ParallelClass.RunParallelForBreak(50, 1, 15, 500);
                    ParallelClass.RunParallelForBreak(15, 1, 15, 1000);
                    goto default;
                case "3":
                    ParallelClass.RunParallelForStop(50, 1, 15, 500);
                    ParallelClass.RunParallelForStop(15, 1, 15, 1000);
                    goto default;
                default:
                    Console.WriteLine();
                    break;
            }
        }
    }
}