using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

namespace Swagger.CustomAPIDoc;

internal sealed class SwaggerStartupValidator(
    ISwaggerProvider swaggerProvider,
    ILogger<SwaggerStartupValidator> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating Swagger document at startup...");

        try
        {
            swaggerProvider.GetSwagger("v1");
            logger.LogInformation("Swagger document validated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Swagger validation failed at startup.");
            throw;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}