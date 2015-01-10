namespace Cedar.HttpCommandHandling.Serialization
{
    using System;
    using System.IO;

    /// <summary>
    /// Provides extension methods for the <see cref="ISerializer"/> interface.
    /// </summary>
    internal static class SerializerExtensions
    {
        /// <summary>
        /// Deserializes an object from the.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="source">The source string representing the serialized object.</param>
        /// <param name="type">The type to deserialize to.</param>
        /// <returns></returns>
        public static object Deserialize(this ISerializer serializer, string source, Type type)
        {
            using (var reader = new StringReader(source))
            {
                return serializer.Deserialize(reader, type);
            }
        }
    }
}