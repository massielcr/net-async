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

        internal static async Task RunProducerWaitToWriteAsync(Coordinate coordinate)
        {
            Channel<Coordinate> channel = Channel.CreateBounded<Coordinate>(1);

            Task producer = Task.Run(async () =>
            {
                while (coordinate is { Latitude: < 90, Longitude: < 180 }  && await channel.Writer.WaitToWriteAsync())
                {
                    var tempCoordinate = coordinate with
                    {
                        Latitude = coordinate.Latitude + 1.5,
                        Longitude = coordinate.Longitude + 2
                    };

                    if (channel.Writer.TryWrite(item: tempCoordinate))
                    {
                        coordinate = tempCoordinate;
                    }
                }

                channel.Writer.Complete();
            });

            await foreach(Coordinate c in channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"Latitude:{c.Latitude} Longitude:{c.Longitude}");
            }

            await producer;

            Console.WriteLine("done");
        }

        internal static async Task RunMultipleProducersMultipleConsumer(Coordinate coordinate)
        {
            Channel<Coordinate> channel = Channel.CreateUnbounded<Coordinate>(
                new UnboundedChannelOptions
                {
                    SingleReader = false,
                    SingleWriter = false
                }
            );

            Task[] producerTasks = Enumerable.Range(0, 3).Select(i => ProduceAsync(i, channel)).ToArray();
            Task[] consumerTasks = Enumerable.Range(0, 2).Select(_ => ConsumeAsync(channel)).ToArray();


            await Task.WhenAll(producerTasks);
            channel.Writer.Complete();


            await Task.WhenAll(consumerTasks);


            static async Task ProduceAsync(int i, Channel<Coordinate> channel)
            {
                Coordinate coordinate = new( Latitude: -90 + (i * 30), Longitude: -180 + (i * 60));

                while (coordinate is { Latitude: < 90, Longitude: < 180 })
                {
                    coordinate = coordinate with
                    {
                        Latitude = coordinate.Latitude + 1.5,
                        Longitude = coordinate.Longitude + 2
                    };

                    await channel.Writer.WriteAsync(coordinate);
                }
            }

            static async Task ConsumeAsync(Channel<Coordinate> channel)
            {
                await foreach(Coordinate c in channel.Reader.ReadAllAsync())
                {
                    Console.WriteLine($"Latitude:{c.Latitude} Longitude:{c.Longitude}");
                }

                Console.WriteLine("done");
            }
        }
    }

    public readonly record struct Coordinate(double Latitude, double Longitude);
}
