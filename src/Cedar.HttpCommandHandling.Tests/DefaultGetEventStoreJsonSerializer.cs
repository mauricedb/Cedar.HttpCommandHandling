namespace Cedar.HttpCommandHandling
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    internal class DefaultGetEventStoreJsonSerializer
    {
        private readonly JsonSerializer _jsonSerializer;

        internal DefaultGetEventStoreJsonSerializer()
        {
            _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
            });
        }

        public object Deserialize(TextReader reader, Type type)
        {
            return _jsonSerializer.Deserialize(reader, type);
        }

        public void Serialize(TextWriter writer, object source)
        {
            _jsonSerializer.Serialize(writer, source);
        }
    }
}