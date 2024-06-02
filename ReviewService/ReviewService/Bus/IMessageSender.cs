namespace ReviewService.Bus
{
    public interface IMessageSender : IAsyncDisposable
    {
        Task SendMessageAsync(string messageBody);
    }
}
