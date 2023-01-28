using MakoIoT.Device.Services.Messaging.MessageConverters;
using MakoIoT.Messages;
using nanoFramework.Json;
using nanoFramework.TestFramework;
using System;
using System.Text;

namespace MakoIoT.Device.Services.Messaging.Test.MessageConverters
{
    [TestClass]
    public class IMessageConvertTest
    {
        public class TestMessage : IMessage
        {
            public TestMessage()
            {
                MessageType = this.GetType().AssemblyQualifiedName;
            }
            public string Text { get; set; }
            public string MessageType { get; set; }
        }

        public class TestMessageNested : IMessage
        {
            public TestMessageNested()
            {
                MessageType = this.GetType().AssemblyQualifiedName;
            }
            public string Text { get; set; }
            public string MessageType { get; set; }
            public TestMessage MessageInternal { get; set; }
        }

        [TestMethod]
        public void ToType_Should_ReturnValidObject()
        {
            var textToCheck = "Hello!";
            var inputData = new JsonObject();
            inputData.Add(nameof(IMessage.MessageType), new JsonValue("MakoIoT.Device.Services.Messaging.Test.MessageConverters.IMessageConvertTest+TestMessage, NFUnitTest, Version=0.0.0.0"));
            inputData.Add(nameof(TestMessage.Text), new JsonValue(textToCheck));
            var converter = new IMessageConvert();

            var output = converter.ToType(inputData);

            Assert.AreEqual(output.GetType().FullName, typeof(TestMessage).FullName);
            var asMessage = (TestMessage)output;
            Assert.AreEqual(asMessage.Text, textToCheck);
        }

        [TestMethod]
        public void ToType_Should_ReturnValidObjectForNestedClass()
        {
            var textToCheck = "Hello!";
            var internalMessageText = "Hello2!";
            var inputData = new JsonObject();
            inputData.Add(nameof(IMessage.MessageType), new JsonValue("MakoIoT.Device.Services.Messaging.Test.MessageConverters.IMessageConvertTest+TestMessageNested, NFUnitTest, Version=0.0.0.0"));
            inputData.Add(nameof(TestMessage.Text), new JsonValue(textToCheck));
            var internalMessage = new JsonObject();
            internalMessage.Add(nameof(TestMessage.Text), new JsonValue(internalMessageText));
            inputData.Add(nameof(TestMessageNested.MessageInternal), internalMessage);
            var converter = new IMessageConvert();

            var output = converter.ToType(inputData);

            Assert.AreEqual(output.GetType().FullName, typeof(TestMessageNested).FullName);
            var asMessage = (TestMessageNested)output;
            Assert.AreEqual(asMessage.Text, textToCheck);
            Assert.AreEqual(asMessage.MessageInternal.Text, internalMessageText);
        }

    }
}
