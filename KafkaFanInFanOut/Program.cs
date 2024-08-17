using System;
using System.Threading;
using Confluent.Kafka;

class Program
{
    static void Main(string[] args)
    {
        var consumerConfig = new ConsumerConfig
        {
            GroupId = "test-consumer-group",
            BootstrapServers = "localhost:9092", // Replace with your Kafka server address
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092" // Replace with your Kafka server address
        };

        using (var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build())
        using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
        {
            consumer.Subscribe("ingestion-topic");

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // Prevent the process from terminating.
                cts.Cancel();
            };

            try
            {
                while (true)
                {
                    var consumeResult = consumer.Consume(cts.Token);
                    Console.WriteLine(
                        $"Consumed message '{consumeResult.Message.Value}' from '{consumeResult.TopicPartitionOffset}'."
                    );

                    var random = new Random();
                    var randomPartition = random.Next(0, 20);
                    // Specify the partition you want to produce to
                    var partition = new Partition(randomPartition); // Change this value to the desired partition number

                    // Produce the message to the specified partition
                    var message = new Message<Null, string> { Value = consumeResult.Message.Value };
                    var topicPartition = new TopicPartition("partition-ingestion-topic", partition);

                    producer.Produce(
                        topicPartition,
                        message,
                        deliveryReport =>
                        {
                            if (deliveryReport.Error.IsError)
                            {
                                Console.WriteLine(
                                    $"Failed to deliver message: {deliveryReport.Error.Reason}"
                                );
                            }
                            else
                            {
                                Console.WriteLine(
                                    $"Delivered message to {deliveryReport.TopicPartitionOffset}"
                                );
                            }
                        }
                    );

                    // You can also wait for the delivery report (synchronous produce)
                    // producer.Produce(topicPartition, message).Wait();
                }
            }
            catch (OperationCanceledException)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                consumer.Close();
            }
        }
    }
}
