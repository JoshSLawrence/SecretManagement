using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using SecretManagement.Interfaces;

namespace SecretManagement.Services;

public class ApplicationService : IService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApplicationService> _logger;
    private readonly GraphServiceClient _graphClient;

    private int _totalApplications = 0;
    private int _totalCredentials = 0;
    private int _totalExpiredCredentials = 0;

    public ApplicationService(IConfiguration configuration, ILogger<ApplicationService> logger, GraphServiceClient graphClient)
    {
        _configuration = configuration;
        _logger = logger;
        _graphClient = graphClient;
    }

    public async Task Start()
    {
        _logger.LogInformation("ApplicationService started.");
        var me = await _graphClient.Me.GetAsync();
        var applications = await _graphClient.Applications.GetAsync(requestConfiguration =>
        {
            requestConfiguration.QueryParameters.Top = 999;
            requestConfiguration.QueryParameters.Select =
            [
                "displayName",
                "id",
                "appid",
                "keyCredentials",
                "passwordCredentials",
            ];
        });

        if (applications is null)
        {
            _logger.LogWarning("No applications found.");
            return;
        }

        var pageIterator = PageIterator<Application, ApplicationCollectionResponse>
            .CreatePageIterator(
            _graphClient,
            applications,
            (app) =>
            {
                _totalApplications++;

                if (app.KeyCredentials is not null)
                {
                    foreach (var keyCredential in app.KeyCredentials)
                    {
                        _totalCredentials++;

                        bool hasExpired = keyCredential.EndDateTime.HasValue && keyCredential.EndDateTime.Value < DateTime.UtcNow;

                        switch (hasExpired)
                        {
                            case true:
                                _totalExpiredCredentials++;
                                _logger.LogDebug("Application: {DisplayName}, KeyCredential: {KeyCredentialDisplayName}, Expiry: {KeyCredentialExpiry}, HasExpired: {HasExpired}",
                                    app.DisplayName,
                                    keyCredential.DisplayName,
                                    keyCredential.EndDateTime?.ToString(),
                                    hasExpired);
                                break;

                            case false:
                                _logger.LogDebug("Application: {DisplayName}, KeyCredential: {KeyCredentialDisplayName}, Expiry: {KeyCredentialExpiry}, HasExpired: {HasExpired}",
                                    app.DisplayName,
                                    keyCredential.DisplayName,
                                    keyCredential.EndDateTime?.ToString(),
                                    hasExpired);
                                break;
                        }
                    }
                }

                if (app.PasswordCredentials is not null)
                {
                    foreach (var passwordCredential in app.PasswordCredentials)
                    {
                        _totalCredentials++;

                        bool hasExpired = passwordCredential.EndDateTime.HasValue && passwordCredential.EndDateTime.Value < DateTime.UtcNow;

                        switch (hasExpired)
                        {
                            case true:
                                _totalExpiredCredentials++;
                                _logger.LogDebug("Application: {DisplayName}, PasswordCredential: {PasswordCredentialDisplayName}, Expiry: {PasswordCredentialExpiry}, HasExpired: {HasExpired}",
                                    app.DisplayName,
                                    passwordCredential.DisplayName,
                                    passwordCredential.EndDateTime?.ToString(),
                                    hasExpired);
                                break;

                            case false:
                                _logger.LogDebug("Application: {DisplayName}, PasswordCredential: {PasswordCredentialDisplayName}, Expiry: {PasswordCredentialExpiry}, HasExpired: {HasExpired}",
                                    app.DisplayName,
                                    passwordCredential.DisplayName,
                                    passwordCredential.EndDateTime?.ToString(),
                                    hasExpired);
                                break;
                        }
                    }
                }

                return true;
            });

        await pageIterator.IterateAsync();
    }

    public void Stop()
    {
        _logger.LogInformation($"Total applications: {_totalApplications}");
        _logger.LogInformation($"Total credentials: {_totalCredentials}");
        _logger.LogInformation($"Total expired credentials: {_totalExpiredCredentials}");
        _logger.LogInformation("ApplicationService stopped.");
    }
}
