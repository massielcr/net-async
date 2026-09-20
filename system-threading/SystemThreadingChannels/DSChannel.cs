using System.Threading.Channels;

namespace SystemThreadingChannels
{
    internal static class DSChannel
    {
        internal static async Task Run(int itemsCount)
        {

            Channel<int> channel = Channel.CreateUnbounded<int>();

            Task producer = ProduceAsync(channel.Writer, itemsCount);
            Task consumer = ConsumeAsync(channel.Reader);


            static async Task ProduceAsync(ChannelWriter<int> writer, int count)
            {
                for(int i = 0; i < count; i++)
                {
                    await writer.WriteAsync(i);
                }

                writer.Complete();
            }

            static async Task ConsumeAsync(ChannelReader<int> reader)
            {
                await foreach(int  item in reader.ReadAllAsync())
                {
                    Console.WriteLine($"Received: {item}");
                }

                Console.WriteLine("done");
            }
        }
    }
}
