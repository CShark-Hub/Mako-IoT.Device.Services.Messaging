using System.Collections;
using System.Diagnostics;
using MakoIoT.Messages;
using nanoFramework.Json;
using nanoFramework.TestFramework;

namespace MakoIoT.Device.Services.Messaging.Test
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void DeserializeObject_given_type_mapping_should_create_object_with_concrete_type()
        {
            string messageText = "{\"MessageId\":\"message-id\",\"OriginTime\":\"2022-01-01T00:00:00.000Z\",\"Message\":{\"Text\":\"Hello!\",\"MessageType\":\"MakoIoT.Device.Services.Messaging.Test.SerializationTest+TestMessage, NFUnitTest, Version=1.0.0.0\"},\"SenderAddress\":\"127.0.0.1\",\"Sender\":\"test\"}";

            var envelope = (Envelope)JsonConvert.DeserializeObject(messageText, typeof(Envelope), new Hashtable
            {
                {typeof(IMessage), nameof(IMessage.MessageType)}
            });

            Debug.WriteLine(envelope.Message.GetType().FullName);


            Assert.IsType(typeof(TestMessage), envelope.Message);
            Assert.Equal("Hello!", ((TestMessage)envelope.Message).Text);

        }

        // [TestMethod]
        // public void SerializeObject_given_proprertyTypeMapping_should_serialize_properties_of_concrete_type()
        // {
        //     var m = new Envelope { Message = new BlinkCommand{LedOn = true} };
        //
        //     string messageText = JsonConvert.SerializeObject(m);//, new Hashtable { { nameof(Envelope.Message), typeof(BlinkCommand) } });
        //
        //     Debug.WriteLine(messageText);
        //
        //     Assert.Contains("\"LedOn\":true", messageText);
        // }


        // [TestMethod]
        // public void DeserializeObject_given_type_mapping_from_external_assembly_should_create_object_with_concrete_type()
        // {
        //     string messageText = "{\"MessageId\":\"message-id\",\"OriginTime\":\"2022-01-01T00:00:00.000Z\",\"Message\":{\"LedOn\":true,\"MessageType\":\"Mako.IoT.Demo.Shared.Messages.Commands.BlinkCommand\"},\"SenderAddress\":\"127.0.0.1\",\"Sender\":\"test\"}";
        //
        //     var envelope = (Envelope)JsonConvert.DeserializeObject(messageText, typeof(Envelope), new Hashtable
        //     {
        //         {typeof(IMessage), nameof(IMessage.MessageType)}
        //     });
        //
        //     Debug.WriteLine(envelope.Message.GetType().FullName);
        //
        //
        //     Assert.IsType(typeof(BlinkCommand), envelope.Message);
        //     Assert.Equal(true, ((BlinkCommand)envelope.Message).LedOn);
        //
        // }

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
