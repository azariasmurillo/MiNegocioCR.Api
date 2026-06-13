using MiNegocioCR.Api.Application.Interfaces.Variants;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class ImageImportBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ImageImportBackgroundService> _logger;

    public ImageImportBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ImageImportBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Image import worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<IImageImportBatchProcessor>();
                await processor.ProcessNextAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image import worker iteration failed.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("Image import worker stopped.");
    }
}
