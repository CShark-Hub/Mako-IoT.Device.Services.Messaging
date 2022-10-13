using System;
using MakoIoT.Device.Services.Interface;

namespace MakoIoT.Device.Services.Messaging.Test.Mocks
{
    public class MockCommunicationService : ICommunicationService
    {
        public string ClientName => "TestClient";
        public event EventHandler MessageReceived;
        public bool CanSend { get; set; }
        public string ClientAddress => "127.0.0.1";

        public string[] Subscriptions { get; private set; }

        public string MessageSent { get; private set; }
        public string TopicSent { get; private set; }

        public void Connect(string[] subscriptions)
        {
            Subscriptions = subscriptions;
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Send(string messageString, string recipient)
        {
            MessageSent = messageString;
            TopicSent = recipient;
        }

        public void Publish(string messageString, string messageType)
        {
            MessageSent = messageString;
            TopicSent = messageType;
        }

        public void InvokeMessageReceived(string messageString)
        {
            MessageReceived(this, new ObjectEventArgs(messageString));
        }

    }
}
