using SystemThreadingChannels;

class Program
{
    static async Task Main()
    {
        while (true)
        {
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("[1]  - Channel<T>");

            string? key = Console.ReadLine();

            switch (key)
            {
                case "1":
                    await DSChannel.Run(10);
                    goto default;
                default:
                    Console.WriteLine();
                    break;
            }
        }
    }
}