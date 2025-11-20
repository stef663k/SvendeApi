using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SvendeApi.Interface;

namespace SvendeApi.Services;

/// <summary>
/// Periodically deletes inactive users (180+ days) via IUserService.
/// </summary>
public class InactiveUserCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InactiveUserCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public InactiveUserCleanupService(
        IServiceProvider serviceProvider,
        ILogger<InactiveUserCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Inactive user cleanup service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cleaning up inactive users.");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Inactive user cleanup service stopping.");
    }

    private async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var deleted = await userService.DeleteInactiveUsersAsync();

        _logger.LogInformation("Inactive user cleanup completed. Deleted {Count} users.", deleted);
    }
}

