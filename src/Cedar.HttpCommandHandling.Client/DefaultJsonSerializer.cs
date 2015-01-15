namespace Cedar.HttpCommandHandling
{
    using System;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal static class DefaultJsonSerializer
    {
        internal static readonly JsonSerializer Instance = JsonSerializer.Create(new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.None,
        });

        internal static object Deserialize(this JsonSerializer serializer, string source, Type type)
        {
            using (var reader = new StringReader(source))
            {
                return serializer.Deserialize(reader, type);
            }
        }

        internal static string Serialize(this JsonSerializer serializer, object o)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, o);
                return sb.ToString();
            }
        }
    }
}