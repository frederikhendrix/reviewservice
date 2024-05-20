using Azure.Messaging.ServiceBus;
using System.Text;

namespace ReviewService.Bus
{
    public class MessageReceiver
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;

        public MessageReceiver(string serviceBusConnectionString, string topicName, string subscriptionName)
        {
            _client = new ServiceBusClient(serviceBusConnectionString);
            _processor = _client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions());
        }

        public async Task RegisterOnMessageHandlerAndReceiveMessagesAsync()
        {
            _processor.ProcessMessageAsync += ProcessMessagesAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            await _processor.StartProcessingAsync();
        }

        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            try
            {
                string body = Encoding.UTF8.GetString(args.Message.Body);
                Console.WriteLine($"Received message: {body}");

                // Complete the message
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                // If an error occurs, abandon the message to make it available again for processing
                await args.AbandonMessageAsync(args.Message);
                Console.WriteLine($"Message processing failed: {ex.Message}");
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"Message handler encountered an exception: {args.Exception}.");
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await _processor.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
