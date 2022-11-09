using MakoIoT.Device.Services.Interface;
using MakoIoT.Device.Services.Messaging.Test.Mocks;
using MakoIoT.Messages;
using nanoFramework.TestFramework;

namespace MakoIoT.Device.Services.Messaging.Test
{
    [TestClass]
    public class MessageBusTests
    {
        [TestMethod]
        public void DeserializeObject_given_type_mapping_should_create_object_with_concrete_type()
        {
            var messageText = "{\"MessageId\":\"message-id\",\"OriginTime\":\"2022-01-01T00:00:00.000Z\",\"Message\":{\"Text\":\"Hello!\",\"MessageType\":\"MakoIoT.Device.Services.Messaging.Test.SerializationTest+TestMessage, NFUnitTest, Version=1.0.0.0\"},\"SenderAddress\":\"127.0.0.1\",\"Sender\":\"test\"}";
            var msgBus = new MessageBus(new MockCommunicationService(), new MockLogger(), new MessageBusOptions());
            msgBus.OnMessageReceived(this, new ObjectEventArgs(messageText));
            
            // TODO: Consumer + assert 
        }

        public class TestMessage : IMessage
        {
            public TestMessage()
            {
                MessageType = this.GetType().AssemblyQualifiedName;
            }
            public string Text { get; set; }
            public string MessageType { get; set; }
        }
    }
}
