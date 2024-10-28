using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;

public interface IHotelInfoService
{
    Task<string> GetHotelNameAsync();
    Task<string> GetHotelContactAsync();
    Task<string> GetHotelAddressline1Async();
    Task<string> GetHotelAddressline2Async();
    Task<string> GetHotelEmailAsync();

}

public class HotelInfoService : IHotelInfoService
{
    private readonly RazorPagesMovieContext _context;

    public HotelInfoService(RazorPagesMovieContext context)
    {
        _context = context;
    }

    public async Task<string> GetHotelNameAsync()
    {
        // Adjust this to match your SysPara table structure
        var hotelName = await _context.SysPara
            .Where(p => p.Key == "HOTELNAME")
            .Select(p => p.Value) // Assuming hotel name is stored in ParameterValue
            .FirstOrDefaultAsync();

        return hotelName ?? "Default Hotel Name"; // Fallback if no name is found
    }

    public async Task<string> GetHotelContactAsync()
    {
        // Adjust this to match your SysPara table structure
        var hotelName = await _context.SysPara
            .Where(p => p.Key == "PHONE")
            .Select(p => p.Value) // Assuming hotel name is stored in ParameterValue
            .FirstOrDefaultAsync();

        return hotelName ?? "Phone No"; // Fallback if no name is found
    }

    public async Task<string> GetHotelAddressline1Async()
    {
        // Adjust this to match your SysPara table structure
        var hotelName = await _context.SysPara
            .Where(p => p.Key == "ADDRESS1")
            .Select(p => p.Value) // Assuming hotel name is stored in ParameterValue
            .FirstOrDefaultAsync();

        return hotelName ?? "Address 1"; // Fallback if no name is found
    }

    public async Task<string> GetHotelAddressline2Async()
    {
        // Adjust this to match your SysPara table structure
        var hotelName = await _context.SysPara
            .Where(p => p.Key == "ADDRESS2")
            .Select(p => p.Value) // Assuming hotel name is stored in ParameterValue
            .FirstOrDefaultAsync();

        return hotelName ?? "Address 2"; // Fallback if no name is found
    }

    public async Task<string> GetHotelEmailAsync()
    {
        // Adjust this to match your SysPara table structure
        var hotelName = await _context.SysPara
            .Where(p => p.Key == "EMAIL")
            .Select(p => p.Value) // Assuming hotel name is stored in ParameterValue
            .FirstOrDefaultAsync();

        return hotelName ?? "email"; // Fallback if no name is found
    }
}
