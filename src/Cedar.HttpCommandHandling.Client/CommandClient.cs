namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.HttpCommandHandling.Logging;

    public static class CommandClient
    {
        private static readonly ILog Logger = LogProvider.GetLogger("Cedar.HttpCommandHandling.CommandClient");
        internal static readonly IJsonSerializerStrategy JsonSerializerStrategy = new CamelCasingSerializerStrategy();

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
                var problemDetails = SimpleJson
                    .DeserializeObject<HttpProblemDetails>(body, JsonSerializerStrategy);
                throw new HttpProblemDetailsException(problemDetails);
            }
            response.EnsureSuccessStatusCode();
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