namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.HttpCommandHandling.Logging;

    public static class CommandClient
    {
        private static readonly ILog Logger = LogProvider.GetLogger("Cedar.HttpCommandHandling.CommandClient");
        internal static readonly ProductInfoHeaderValue UserAgent;
        internal static readonly IJsonSerializerStrategy JsonSerializerStrategy = new CamelCasingSerializerStrategy();
        internal const string HttpProblemDetailsClrType = "Cedar-HttpProblemDetails-ClrType";
        internal const string HttpProblemDetailsExceptionClrType = "Cedar-HttpProblemDetailsException-ClrType";

        static CommandClient()
        {
            var type = typeof(CommandClient);
            var version = type.Assembly.GetName().Version;
            UserAgent = new ProductInfoHeaderValue(type.FullName, version.Major + "." + version.Minor);
        }

        public static Task PutCommand(this HttpClient client, object command, Guid commandId)
        {
            return PutCommand(client, command, commandId, string.Empty);
        }

        public static async Task PutCommand(this HttpClient client, object command, Guid commandId, string basePath)
        {
            var request = CreatePutCommandRequest(command, commandId, basePath);

            Logger.InfoFormat("Put Command {0}. Type: {1}", commandId, command.GetType());
            HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            Logger.InfoFormat("Put Command {0}. Response: {1}", commandId, response.ReasonPhrase);

            await response.EnsureCommandSuccess();
        }

        public static HttpRequestMessage CreatePutCommandRequest(object command, Guid commandId, string basePath)
        {
            string commandJson = SimpleJson.SerializeObject(command, JsonSerializerStrategy);
            var httpContent = new StringContent(commandJson);
            httpContent.Headers.ContentType =
                MediaTypeHeaderValue.Parse("application/vnd.{0}+json".FormatWith(command.GetType().FullName.ToLowerInvariant()));

            var request = new HttpRequestMessage(HttpMethod.Put, basePath + "/{0}".FormatWith(commandId))
            {
                Content = httpContent
            };
            request.Headers.UserAgent.Add(UserAgent);
            request.Headers.Accept.Add(HttpProblemDetails.MediaTypeWithQualityHeaderValue);

            return request;
        }

        public static async Task EnsureCommandSuccess(this HttpResponseMessage response)
        {
            if ((int)response.StatusCode >= 400
                && response.Content.Headers.ContentType != null
                && response.Content.Headers.ContentType.Equals(HttpProblemDetails.MediaTypeHeaderValue))
            {
                // Extract problem details, if they are supplied.
                var body = await response.Content.ReadAsStringAsync();
                var problemDetailsClrType = response
                    .Headers
                    .Single(kvp => kvp.Key == HttpProblemDetailsClrType)
                    .Value
                    .Single();
                var exceptionClrType = response
                    .Headers
                    .Single(kvp => kvp.Key == HttpProblemDetailsExceptionClrType)
                    .Value
                    .Single();

                var problemDetailsType = GetType(problemDetailsClrType);
                var problemDetails = SimpleJson.DeserializeObject(body, problemDetailsType, JsonSerializerStrategy);

                var exceptionType = GetType(exceptionClrType);
                var exception = (Exception)Activator.CreateInstance(exceptionType, problemDetails);

                throw exception;
            }

            response.EnsureSuccessStatusCode();
        }

        private static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if(type == null)
            {
                throw new TypeLoadException("Failed to get type {0}".FormatWith(typeName));
            }
            return type;
        }

        private class CamelCasingSerializerStrategy : PocoJsonSerializerStrategy
        {
            protected override string MapClrMemberNameToJsonFieldName(string clrPropertyName)
            {
                return clrPropertyName.ToCamelCase();
            }
        }
    }
}