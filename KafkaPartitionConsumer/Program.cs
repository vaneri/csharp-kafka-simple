using System;
using System.Threading;
using Confluent.Kafka;

class Program
{
    static void Main(string[] args)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "partition-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        var topic = "partition-ingestion-topic";
        var partitionCount = 20; // Number of partitions

        // Start a consumer for each partition
        var consumers = new IConsumer<Ignore, string>[partitionCount];
        var threads = new Thread[partitionCount];

        for (int i = 0; i < partitionCount; i++)
        {
            int partition = i;
            consumers[partition] = new ConsumerBuilder<Ignore, string>(config).Build();
            consumers[partition].Assign(new[] { new TopicPartition(topic, partition) });

            threads[partition] = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        var cr = consumers[partition].Consume(CancellationToken.None);
                        Console.WriteLine(
                            $"Partition {partition}: Received message '{cr.Message.Value}' at: '{cr.TopicPartitionOffset}'."
                        );

                        // Manually commit offsets if needed
                        consumers[partition].Commit(cr);
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error occurred: {e.Error.Reason}");
                }
                finally
                {
                    consumers[partition].Close();
                }
            });

            threads[partition].Start();
        }

        // Optionally wait for the threads to finish (they typically run indefinitely)
        foreach (var thread in threads)
        {
            thread.Join();
        }
    }
}
