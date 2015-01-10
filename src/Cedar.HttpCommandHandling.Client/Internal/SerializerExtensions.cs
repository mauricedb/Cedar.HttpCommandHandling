namespace Cedar.HttpCommandHandling.Internal
{
    using System;
    using System.IO;

    internal static class SerializerExtensions
    {
        public static object Deserialize(this DefaultJsonSerializer serializer, string source, Type type)
        {
            using (var reader = new StringReader(source))
            {
                return serializer.Deserialize(reader, type);
            }
        }
    }
}