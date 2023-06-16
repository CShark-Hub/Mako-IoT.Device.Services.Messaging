# Mako-IoT.Device.Services.Messaging
Message bus with pub-sub and stronly typed data contracts.

## Main concepts

### Message routing
There are two type of routes:
- **Broadcast** messages are published under _topics_ and delivered to one or more subscribers. This is usually used for propagating events.
- **Direct** message is delivered to single recipient. This is usually used for sending commands.
Routing is done automatically based on message types.

### Message contracts
Message contracts are classes that implement _IMessage_ interface. _MessageType_ must be set to full type name of the class.
```c#
public class BlinkCommand : IMessage
{
    public BlinkCommand()
    {
        MessageType = this.GetType().FullName;
    }
    public bool LedOn { get; set; }
    public string MessageType { get; set; }
}
```

### Consumers
Messages are delivered to consumer classes.
```c#
public class BlinkCommandConsumer : IConsumer
{
    private readonly IBlinker _blinker;
    private readonly ILogger _logger;

    public BlinkCommandConsumer(IBlinker blinker, ILogger logger)
    {
        _blinker = blinker;
        _logger = logger;
    }

    public void Consume(ConsumeContext context)
    {
        var cmd = (BlinkCommand)context.Message;
        _logger.LogDebug($"Setting LED to {cmd.LedOn}");
        _blinker.Set(cmd.LedOn);
    }
}
```
Consumers are registered against message types in _DeviceBuilder_ with _AddDirectMessageConsumer_ or _AddSubscriptionConsumer_
```c#
public static void Main()
{
    DeviceBuilder.Create()
        .AddMessageBus(o =>
        {
            o.AddDirectMessageConsumer(typeof(BlinkCommand), typeof(BlinkCommandConsumer), ConsumeStrategy.LastMessageWins);
        })
   //[...]
```
#### Consume strategies
Consuming multiple messages of same type (i.e. by single consumer) may be done in one of three fashions:
1. Synchronously - receiving subsequent messages is blocked until current message ends processing.
2. FIFO - received messages are put onto a queue and processed in order.
3. Last Message Wins - the most recent message is processed and older unprocessed messages are discarded. This equivalent to bulkhead resilience pattern with single concurrent action.

### Sending messages
```c#
bus.Send(msg, "device1"); //sends direct message to "device1"
bus.Publish(msg); //publishes broadcast message
```

## Interoperability with .NET
With [.NET implementation of message bus](https://github.com/CShark-Hub/Mako-IoT.Core.Services.Messaging) you can easily communicate between nanoFramework devices and .NET services or application. Data contract classes can be shared across both parties. See [messaging sample](https://github.com/CShark-Hub/Mako-IoT.Device.Samples/tree/main/Messaging).
