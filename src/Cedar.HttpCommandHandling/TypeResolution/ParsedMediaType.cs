namespace Cedar.HttpCommandHandling.TypeResolution
{
    public class ParsedMediaType : IParsedMediaType
    {
        public ParsedMediaType(string typeName, int? version, string serializationType)
        {
            TypeName = typeName;
            Version = version;
            SerializationType = serializationType;
        }

        public string TypeName { get; }

        public int? Version { get; }

        public string SerializationType { get; }
    }
}