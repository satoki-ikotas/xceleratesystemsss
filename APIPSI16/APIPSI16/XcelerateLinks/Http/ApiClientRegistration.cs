using System.Net.Http.Headers;

namespace XcelerateLinks.Mvc.Http
{
    public static class ApiClientRegistration
    {
        public static IServiceCollection AddApiClient(this IServiceCollection services, IConfiguration configuration)
        {
            // Read API base URL from configuration with default fallback
            var apiBaseUrl = configuration["Api:BaseUrl"] ?? "http://localhost:5270";
            
            // Ensure base URL ends with slash for relative URIs
            if (!apiBaseUrl.EndsWith('/'))
                apiBaseUrl += '/';

            // Register named HttpClient "Api" with base address and timeout
            services.AddHttpClient("Api", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            return services;
        }
    }
}
