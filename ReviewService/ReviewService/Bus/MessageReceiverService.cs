using Azure.Messaging.ServiceBus;

namespace ReviewService.Bus
{
    public class MessageReceiverService : IHostedService
    {
        private readonly MessageReceiver _messageReceiver;

        public MessageReceiverService(string serviceBusConnectionString, string topicName, string subscriptionName)
        {
            _messageReceiver = new MessageReceiver(serviceBusConnectionString, topicName, subscriptionName);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _messageReceiver.RegisterOnMessageHandlerAndReceiveMessagesAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

}
