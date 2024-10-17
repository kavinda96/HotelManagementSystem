using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection; // To create service scope

public class ExchangeRateUpdater : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly ILogger<ExchangeRateUpdater> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory; // To create a scope for DbContext

    public ExchangeRateUpdater(ILogger<ExchangeRateUpdater> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Execute the update immediately on startup
        UpdateExchangeRate(null);

        // Schedule the next run
        ScheduleNextRun();
        return Task.CompletedTask;
    }

    private void ScheduleNextRun()
    {
        var nextRunTime = GetNextRunTime();
        var delay = nextRunTime - DateTime.UtcNow;

        if (delay < TimeSpan.Zero)
        {
            // If the next run time is in the past, schedule for the next day
            nextRunTime = nextRunTime.AddDays(1);
            delay = nextRunTime - DateTime.UtcNow;
        }

        // Set the timer to trigger the UpdateExchangeRate method at the calculated delay
        _timer = new Timer(UpdateExchangeRate, null, delay, Timeout.InfiniteTimeSpan);
    }

    private DateTime GetNextRunTime()
    {
        // Get the current time in SLT (UTC+5:30)
        var sriLankaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time");
        var now = DateTime.UtcNow;
        var slTime = TimeZoneInfo.ConvertTimeFromUtc(now, sriLankaTimeZone);

        // Set the next run time to today at 9 AM SLT
        var nextRun = new DateTime(slTime.Year, slTime.Month, slTime.Day, 9, 0, 0, DateTimeKind.Unspecified);

        // If it's already past 9 AM SLT, schedule for the next day
        if (slTime >= nextRun)
        {
            nextRun = nextRun.AddDays(1);
        }

        return TimeZoneInfo.ConvertTimeToUtc(nextRun, sriLankaTimeZone);
    }

    private async void UpdateExchangeRate(object state)
    {
        using var client = new HttpClient();
        string apiKey = "ad8507a9a837efbac6968f4d";  // Replace with your real API key
        string url = $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/USD";

        try
        {
            var response = await client.GetStringAsync(url);
            dynamic data = JsonConvert.DeserializeObject(response);
            var lkrRate = (decimal)data.conversion_rates.LKR; // Explicitly cast to decimal

            if(lkrRate > 0)
            {
                SaveExchangeRateToDatabase(lkrRate);
            }
            else
            {
                SaveExchangeRateToDatabase(-1);
            }

            _logger.LogInformation($"Fetched USD to LKR exchange rate: {lkrRate} at {DateTime.UtcNow}");

            // Save exchange rate to the database
            
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching exchange rate: {ex.Message}");
        }
        finally
        {
            // Reschedule the next run after the update
            ScheduleNextRun();
        }
    }

    private void SaveExchangeRateToDatabase(decimal lkrRate)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RazorPagesMovieContext>();

            var exchangeRate = new ExchangeRate
            {
                CreatedDate = DateTime.UtcNow, // Store the time in UTC
                ExchangeRateName = "USD",
                ExchangeRateAmount = lkrRate
            };

            dbContext.ExchangeRate.Add(exchangeRate);  // Add the record
            dbContext.SaveChanges();                  // Save it to the database
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}