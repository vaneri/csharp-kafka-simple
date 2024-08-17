using System;
using System.Threading.Tasks;
using Confluent.Kafka;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092" // Replace with your Kafka server address
        };

        using (var producer = new ProducerBuilder<Null, string>(config).Build())
        {
            try
            {
                while (true)
                {
                    var deliveryResult = await producer.ProduceAsync(
                        "ingestion-topic",
                        new Message<Null, string> { Value = "Hello Kafka!" }
                    );
                    Console.WriteLine(
                        $"Delivered '{deliveryResult.Value}' to '{deliveryResult.TopicPartitionOffset}'"
                    );
                    await Task.Delay(1000); // Delay for 1 second
                }
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }
        }
    }
}
