namespace BrainGames.API.Workers;

public class UpdateGameWorker(ILogger<UpdateGameWorker> logger) : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromMilliseconds(5000);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            logger.LogInformation("Executing PeriodicBackgroundTask");
        }
    }
}