using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using SecretManagement.Interfaces;
using SecretManagement.Services;

namespace SecretManagement;

internal class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var worker = ActivatorUtilities.CreateInstance<Worker>(host.Services);

        await worker.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
        {
            services.AddScoped<IService, ApplicationService>();
            services.AddSingleton(provider =>
            {
                var options = new DefaultAzureCredentialOptions()
                {
                    ExcludeEnvironmentCredential = true
                };
                var credential = new DefaultAzureCredential(options);
                return new GraphServiceClient(credential);
            });
        });
    }
}
