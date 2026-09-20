using System.Reflection.PortableExecutable;
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

        internal static async Task RunProducerTryWrite(Coordinate coordinate)
        {
            Channel<Coordinate> channel = Channel.CreateBounded<Coordinate>(1);

            Task producer = Task.Run(() => {
                while (coordinate is { Latitude: < 90, Longitude: < 189 })
                {
                    var tempCoordinate = coordinate with
                    {
                        Latitude = coordinate.Latitude + 1.5,
                        Longitude = coordinate.Longitude + 2
                    };

                    if (channel.Writer.TryWrite(item: tempCoordinate))  //=> TryWrite returns right away resulting on true/false
                    {
                        coordinate = tempCoordinate;
                    }
                }

                channel.Writer.Complete();
            });

            while (!channel.Reader.Completion.IsCompleted)
            {
                if (channel.Reader.TryRead(out Coordinate c)) //=> TryRead returns right away resulting on true/false
                {
                    Console.WriteLine($"Latitude:{c.Latitude} Longitude:{c.Longitude}");
                }
            }

            await producer;

            Console.WriteLine("done");
        }

        internal static async Task RunProducerWriteAsync(Coordinate coordinate)
        {
            Channel<Coordinate> channel = Channel.CreateBounded<Coordinate>(1);

            Task producer = Task.Run(async () =>
            {
                while(coordinate is { Latitude: < 90, Longitude: < 180})
                {

                    coordinate = coordinate with
                    {
                        Latitude = coordinate.Latitude + 1.5,
                        Longitude = coordinate.Longitude + 2,
                    };

                    await channel.Writer.WriteAsync(coordinate);
                }

                channel.Writer.Complete();
            });

            try
            {
                while (true)
                {
                    Coordinate c = await channel.Reader.ReadAsync();
                    Console.WriteLine($"Latitude:{c.Latitude} Longitude:{c.Longitude}");
                }
            }
            catch(ChannelClosedException)
            {
                Console.WriteLine("done");
            }
            

            await producer;
        }
    }

    public readonly record struct Coordinate(double Latitude, double Longitude);
}
