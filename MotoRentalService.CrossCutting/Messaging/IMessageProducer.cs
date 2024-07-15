namespace MotoRentalService.CrossCutting.Messaging
{
    public interface IMessageProducer
    {
        Task PublishAsync<T>(string topic, T message);
    }
}
