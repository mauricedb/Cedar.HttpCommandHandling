namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Net.Http;
    using CuttingEdge.Conditions;
    using MidFunc = System.Func<System.Func<System.Collections.Generic.IDictionary<string, object>,
            System.Threading.Tasks.Task
        >, System.Func<System.Collections.Generic.IDictionary<string, object>,
            System.Threading.Tasks.Task
        >
    >;

    public static class MidFuncExtensions
    {
        public static HttpClient CreateEmbeddedClient(this MidFunc midFunc, Uri baseAddress = null)
        {
            Condition.Requires(midFunc).IsNotNull();

            baseAddress = baseAddress ?? new Uri("http://localhost");

            var handler = new OwinHttpMessageHandler(midFunc)
            {
                UseCookies = true
            };

            return new HttpClient(handler, true)
            {
                BaseAddress = baseAddress
            };
        }
    }
}