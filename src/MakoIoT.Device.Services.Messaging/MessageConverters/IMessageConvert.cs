using nanoFramework.Json.Converters;
using System;

namespace MakoIoT.Device.Services.Messaging.MessageConverters
{
    // TODO: Tests
    class IMessageConvert : IConverter
    {
        public string ToJson(object value)
        {
            throw new NotImplementedException();
        }

        public object ToType(object value)
        {
            // TODO: Find message type from object
            // TODO: call deserialization with that type
            throw new NotImplementedException();
        }
    }
}
