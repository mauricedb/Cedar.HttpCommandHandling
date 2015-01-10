namespace Cedar.HttpCommandHandling.Internal
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal class DefaultJsonSerializer
    {
        internal static readonly DefaultJsonSerializer Instance;
        internal static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.None
        };

        private readonly JsonSerializer _jsonSerializer;

        static DefaultJsonSerializer()
        {
            Instance = new DefaultJsonSerializer();
        }

        private DefaultJsonSerializer()
        {
            _jsonSerializer = JsonSerializer.Create(Settings);
        }

        public object Deserialize(TextReader reader, Type type)
        {
            return _jsonSerializer.Deserialize(reader, type);
        }
    }
}