namespace Cedar.HttpCommandHandling.Client
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.HttpCommandHandling.Internal;

    public static class HttpClientExtensions
    {
        public static Task PutCommand(this HttpClient client, object command, Guid commandId)
        {
            return PutCommand(client, command, commandId, string.Empty);
        }

        public static async Task PutCommand(this HttpClient client, object command, Guid commandId, string basePath)
        {
            var request = CreatePutRequest(command, commandId, basePath);

            HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            await response.ThrowOnErrorStatus();
        }

        private static HttpRequestMessage CreatePutRequest(object command, Guid commandId, string basePath)
        {
            string commandJson = DefaultJsonSerializer.Instance.Serialize(command);
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

        private static async Task ThrowOnErrorStatus(this HttpResponseMessage response)
        {
            if ((int)response.StatusCode >= 400
                && response.Content.Headers.ContentType != null
                && response.Content.Headers.ContentType.Equals(HttpProblemDetails.MediaTypeHeaderValue)
                && response.Headers.Contains(HttpProblemDetails.HttpProblemDetailsTypeHeader))
            {
                // Extract problem details, if they are supplied.
                var problemDetailTypeName = response.Headers.GetValues(HttpProblemDetails.HttpProblemDetailsTypeHeader)
                    .Single();

                var problemDetailType = Type.GetType(problemDetailTypeName);
                var body = await response.Content.ReadAsStringAsync();
                object problemDetails = DefaultJsonSerializer.Instance.Deserialize(body, problemDetailType);

                throw new HttpProblemDetailsException((HttpProblemDetails)problemDetails);
            }
            response.EnsureSuccessStatusCode();
        }
    }
}