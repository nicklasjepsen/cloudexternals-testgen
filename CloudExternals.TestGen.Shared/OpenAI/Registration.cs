using System;
using Microsoft.Extensions.DependencyInjection;

namespace CloudExternals.TestGen.Shared.OpenAI
{
    public static class Registration
    {
        public static IServiceCollection AddRequiredServices(this IServiceCollection services, string apiKey)
        {
            services.AddHttpClient("OpenAI", client =>
            {
                client.BaseAddress = new Uri("https://api.openai.com");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            });

            services.AddSingleton<CodeParser>();
            services.AddSingleton<MarkdownParser>();
            services.AddSingleton<OpenAIService>();
            services.AddSingleton<TestGenerator>();

            return services;
        }
    }
}
