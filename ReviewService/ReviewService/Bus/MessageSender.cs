using Azure.Messaging.ServiceBus;
using System.Text;

namespace ReviewService.Bus
{
    public class MessageSender
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public MessageSender(string serviceBusConnectionString, string topicName)
        {
            _client = new ServiceBusClient(serviceBusConnectionString);
            _sender = _client.CreateSender(topicName);
        }

        public async Task SendMessageAsync(string messageBody)
        {
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));
            await _sender.SendMessageAsync(message);
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
