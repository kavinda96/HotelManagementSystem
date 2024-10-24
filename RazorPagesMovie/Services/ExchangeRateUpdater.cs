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
    private Timer _timerUSDToLKR;
    private Timer _timerLKRToUSD;
    private readonly ILogger<ExchangeRateUpdater> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ExchangeRateUpdater(ILogger<ExchangeRateUpdater> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Execute both updates immediately on startup
        //UpdateExchangeRate(null);
        //UpdateExchangeRateLKRUSD(null);

        // Schedule the next runs for both methods
        ScheduleNextRunForUSDToLKR();
        ScheduleNextRunForLKRToUSD();

        return Task.CompletedTask;
    }

    // Schedule USD to LKR updates
    private void ScheduleNextRunForUSDToLKR()
    {
        var nextRunTime = GetNextRunTime();
        var delay = nextRunTime - DateTime.UtcNow;

        if (delay < TimeSpan.Zero)
        {
            nextRunTime = nextRunTime.AddDays(1);
            delay = nextRunTime - DateTime.UtcNow;
        }

        _timerUSDToLKR = new Timer(UpdateExchangeRate, null, delay, Timeout.InfiniteTimeSpan);
    }

    // Schedule LKR to USD updates
    private void ScheduleNextRunForLKRToUSD()
    {
        var nextRunTime = GetNextRunTime();
        var delay = nextRunTime - DateTime.UtcNow;

        if (delay < TimeSpan.Zero)
        {
            nextRunTime = nextRunTime.AddDays(1);
            delay = nextRunTime - DateTime.UtcNow;
        }

        _timerLKRToUSD = new Timer(UpdateExchangeRateLKRUSD, null, delay, Timeout.InfiniteTimeSpan);
    }

    private DateTime GetNextRunTime()
    {
        var sriLankaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time");
        var now = DateTime.UtcNow;
        var slTime = TimeZoneInfo.ConvertTimeFromUtc(now, sriLankaTimeZone);
        var nextRun = new DateTime(slTime.Year, slTime.Month, slTime.Day, 9, 0, 0, DateTimeKind.Unspecified);

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
            var lkrRate = (decimal)data.conversion_rates.LKR;

            if (lkrRate > 0)
            {
                SaveExchangeRateToDatabase(lkrRate, 1);
                await UpdateRoomPrices(lkrRate);
            }
            else
            {
                SaveExchangeRateToDatabase(-1, 1);
            }

            _logger.LogInformation($"Fetched USD to LKR exchange rate: {lkrRate} at {DateTime.UtcNow}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching exchange rate: {ex.Message}");
        }
        finally
        {
            ScheduleNextRunForUSDToLKR();
        }
    }

    private async void UpdateExchangeRateLKRUSD(object state)
    {
        using var client = new HttpClient();
        string apiKey = "ad8507a9a837efbac6968f4d";  // Replace with your real API key
        string url = $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/LKR";

        try
        {
            var response = await client.GetStringAsync(url);
            dynamic data = JsonConvert.DeserializeObject(response);
         //   var usdRate = (decimal)data.conversion_rates.USD;
            decimal usdRate = Convert.ToDecimal(data.conversion_rates.USD);

            if (usdRate < 1)
            {
                SaveExchangeRateToDatabase(usdRate, 2);
                await UpdateFoodUsdPrices(usdRate);
            }
            else
            {
                SaveExchangeRateToDatabase(-1, 2);
            }

            _logger.LogInformation($"Fetched LKR to USD exchange rate: {usdRate} at {DateTime.UtcNow}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching exchange rate: {ex.Message}");
        }
        finally
        {
            ScheduleNextRunForLKRToUSD();
        }
    }

    //private void SaveExchangeRateToDatabase(decimal rate, int type)
    //{
    //    using (var scope = _serviceScopeFactory.CreateScope())
    //    {
    //        var dbContext = scope.ServiceProvider.GetRequiredService<RazorPagesMovieContext>();
    //        var exchangeRate = new ExchangeRate
    //        {
    //            CreatedDate = DateTime.UtcNow,
    //            ExchangeRateName = type == 1 ? "USD" : "LKR",
    //            ExchangeRateAmount = rate
    //        };

    //        dbContext.ExchangeRate.Add(exchangeRate);
    //        dbContext.SaveChanges();
    //    }
    //}



    private void SaveExchangeRateToDatabase(decimal rate, int type)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RazorPagesMovieContext>();
            var exchangeRate = new ExchangeRate
            {
                CreatedDate = DateTime.UtcNow,
                ExchangeRateName = type == 1 ? "USD" : "LKR",
                ExchangeRateAmount = rate
            };

            _logger.LogInformation($"Attempting to save exchange rate: {rate} for {exchangeRate.ExchangeRateName}.");

            dbContext.ExchangeRate.Add(exchangeRate);
            dbContext.SaveChanges();

            // Log the value after the save to ensure it matches
            _logger.LogInformation($"Saved exchange rate: {exchangeRate.ExchangeRateAmount}");
        }
    }


    private async Task UpdateRoomPrices(decimal lkrRate)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RazorPagesMovieContext>();
            var rooms = dbContext.Room.ToList();

            foreach (var room in rooms)
            {
                room.Price = room.PriceUSD * lkrRate;
            }

            dbContext.Room.UpdateRange(rooms);
            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Room prices updated based on the new USD to LKR exchange rate.");
        }
    }

    private async Task UpdateFoodUsdPrices(decimal usdRate)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RazorPagesMovieContext>();
            var foods = dbContext.Food.ToList();

            foreach (var foodItem in foods)
            {
                foodItem.PriceUSD = foodItem.Price * usdRate;
            }

            dbContext.Food.UpdateRange(foods);
            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Food USD prices updated based on the new LKR to USD exchange rate.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timerUSDToLKR?.Change(Timeout.Infinite, 0);
        _timerLKRToUSD?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timerUSDToLKR?.Dispose();
        _timerLKRToUSD?.Dispose();
    }
}
