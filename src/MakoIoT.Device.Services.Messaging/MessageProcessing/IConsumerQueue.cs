using MakoIoT.Device.Services.Interface;

namespace MakoIoT.Device.Services.Messaging.MessageProcessing
{
    public interface IConsumerQueue
    {
        public void Consume(ConsumeContext context);
    }
}
