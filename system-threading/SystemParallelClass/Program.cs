using SystemParallelClass;

class Program
{
    static async Task Main()
    {
        while (true)
        {
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("[1]  - Parallel");

            string? key = Console.ReadLine();

            switch (key)
            {
                case "1":
                    ParallelClass.RunParallel(3);
                    goto default;
                default:
                    Console.WriteLine();
                    break;
            }
        }
    }
}