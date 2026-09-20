using SystemThreadingChannels;

class Program
{
    static async Task Main()
    {
        while (true)
        {
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("[1]  - Channel<T>");
            Console.WriteLine("[2]  - TryWrite");

            string? key = Console.ReadLine();

            switch (key)
            {
                case "1":
                    await DSChannel.Run(10);
                    goto default;
                case "2":
                    await DSChannel.RunProducerTryWrite(new Coordinate(Latitude: 85, Longitude: 173));
                    goto default;
                default:
                    Console.WriteLine();
                    break;
            }
        }
    }
}