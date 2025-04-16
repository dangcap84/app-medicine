using System;
using System.Threading;
using System.Threading.Tasks;
using MediTrack.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection; // Required for IServiceScopeFactory
using Microsoft.Extensions.Hosting; // Required for BackgroundService
using Microsoft.Extensions.Logging; // Required for ILogger
using Microsoft.Extensions.Configuration; // Required for IConfiguration

namespace MediTrack.Infrastructure.BackgroundServices
{
    public class NotificationGenerationService : BackgroundService
    {
        private readonly ILogger<NotificationGenerationService> _logger;
        private readonly IServiceScopeFactory _scopeFactory; // Use scope factory for DbContext and scoped services
        private readonly TimeSpan _checkInterval; // How often the service runs
        private readonly TimeSpan _lookAheadTime; // How far ahead to generate notifications

        public NotificationGenerationService(
            ILogger<NotificationGenerationService> logger,
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration) // Inject IConfiguration to read settings
        {
            _logger = logger;
            _scopeFactory = scopeFactory;

            // Read settings from appsettings.json (provide defaults)
            _checkInterval = TimeSpan.FromMinutes(configuration.GetValue<int>("NotificationSettings:CheckIntervalMinutes", 5)); // Default: 5 minutes
            _lookAheadTime = TimeSpan.FromHours(configuration.GetValue<int>("NotificationSettings:LookAheadHours", 24)); // Default: 24 hours

             _logger.LogInformation("NotificationGenerationService configured with CheckInterval: {CheckInterval}, LookAheadTime: {LookAheadTime}", _checkInterval, _lookAheadTime);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationGenerationService is starting.");

            stoppingToken.Register(() => _logger.LogInformation("NotificationGenerationService is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("NotificationGenerationService running at: {time}", DateTimeOffset.Now);

                try
                {
                    // Create a new scope for each execution to resolve scoped services like DbContext
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        await notificationService.GenerateNotificationsAsync(_lookAheadTime);
                    }
                     _logger.LogInformation("Notification generation task completed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating notifications.");
                }

                try
                {
                    // Wait for the next interval, respecting the cancellation token
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken was signaled
                    break;
                }
            }

            _logger.LogInformation("NotificationGenerationService has stopped.");
        }
    }
}
