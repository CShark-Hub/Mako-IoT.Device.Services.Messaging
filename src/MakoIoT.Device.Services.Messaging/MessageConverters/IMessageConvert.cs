using MakoIoT.Messages;
using nanoFramework.Json;
using nanoFramework.Json.Converters;
using System;
using System.Collections;

namespace MakoIoT.Device.Services.Messaging.MessageConverters
{
    sealed class IMessageConvert : IConverter
    {
        private const string MessageTypeKey = nameof(IMessage.MessageType);
        public string ToJson(object value)
        {
            throw new NotImplementedException();
        }

        public object ToType(object value)
        {
            var jsonObject = (JsonObject)value;
            var msgTypeJsonValue= (JsonValue)jsonObject.Get(MessageTypeKey).Value;
            var msgTypeString = msgTypeJsonValue.Value.ToString();
            var msgType = Type.GetType(msgTypeString);

            if (msgType == null)
            {
                throw new Exception("Unable to find type.");
            }

            // Convert from Json objects to hashtable
            // Then call serialization to get json string
            // Then call deserialization with proper object
            var hashtable = ExtractKeyValuePairsFromJsonObject(jsonObject);
            var sectionAsJson = JsonConvert.SerializeObject(hashtable);
            var obj = JsonConvert.DeserializeObject(sectionAsJson, msgType);
            return obj;
        }
        
        private Hashtable ExtractKeyValuePairsFromJsonObject(JsonObject jsonObject)
        {
            var hashtable = new Hashtable();
            foreach (var item in jsonObject.Members)
            {
                if (item is JsonProperty jsonProperty)
                {
                    var keyJsonProp = jsonProperty.Name;
                    var propValue = HandleValue(jsonProperty.Value);

                    hashtable.Add(keyJsonProp, propValue);
                    continue;
                }

                throw new NotSupportedException();
            }

            return hashtable;
        }

        private object HandleValue(JsonToken token)
        {
            if (token is JsonValue value)
            {
                return value.Value;
            }

            if (token is JsonObject obj)
            {
                return ExtractKeyValuePairsFromJsonObject(obj);
            }

            throw new NotSupportedException();
        }
    }
}
