using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RazorPagesMovie.Data;

public interface IHotelInfoService
{
    Task<string> GetHotelNameAsync();
    Task<string> GetHotelContactAsync();
    Task<string> GetHotelAddressline1Async();
    Task<string> GetHotelAddressline2Async();
    Task<string> GetHotelEmailAsync();
    Task<Dictionary<string, string>> GetEmailSettingsAsync();
}

public class HotelInfoService : IHotelInfoService
{
    private readonly RazorPagesMovieContext _context;
    private readonly IConfiguration _configuration;

    public HotelInfoService(RazorPagesMovieContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> GetHotelNameAsync()
    {
        var hotelName = await _context.SysPara
            .Where(p => p.Key == "HOTELNAME")
            .Select(p => p.Value)
            .FirstOrDefaultAsync();

        return hotelName ?? "Default Hotel Name";
    }

    public async Task<string> GetHotelContactAsync()
    {
        var hotelContact = await _context.SysPara
            .Where(p => p.Key == "PHONE")
            .Select(p => p.Value)
            .FirstOrDefaultAsync();

        return hotelContact ?? "Phone No";
    }

    public async Task<string> GetHotelAddressline1Async()
    {
        var addressLine1 = await _context.SysPara
            .Where(p => p.Key == "ADDRESS1")
            .Select(p => p.Value)
            .FirstOrDefaultAsync();

        return addressLine1 ?? "Address 1";
    }

    public async Task<string> GetHotelAddressline2Async()
    {
        var addressLine2 = await _context.SysPara
            .Where(p => p.Key == "ADDRESS2")
            .Select(p => p.Value)
            .FirstOrDefaultAsync();

        return addressLine2 ?? "Address 2";
    }

    public async Task<string> GetHotelEmailAsync()
    {
        var hotelEmail = await _context.SysPara
            .Where(p => p.Key == "EMAIL")
            .Select(p => p.Value)
            .FirstOrDefaultAsync();

        return hotelEmail ?? "email";
    }

    // Fetches hotel-specific email settings with a fallback to default
    public async Task<Dictionary<string, string>> GetEmailSettingsAsync()
    {
        var hotelName = await GetHotelNameAsync();
        var emailSettingsSection = _configuration.GetSection($"EmailSettings:HotelSpecific:{hotelName}");

        // Fallback to default settings if hotel-specific settings are not found
        if (!emailSettingsSection.Exists())
        {
            emailSettingsSection = _configuration.GetSection("EmailSettings:Default");
        }

        return emailSettingsSection.GetChildren()
                                   .ToDictionary(x => x.Key, x => x.Value);
    }
}
