namespace HotelStay.Api.Services;

using HotelStay.Api.Models;
using HotelStay.Api.Providers;

public class HotelSearchService
{
    private readonly IHotelProvider[] _providers;

    public HotelSearchService(params IHotelProvider[] providers)
    {
        _providers = providers;
    }

    public async Task<SearchResponse> SearchHotels(HotelSearchQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.Destination))
            throw new ArgumentException("Destination is required");

        if (query.CheckOut <= query.CheckIn)
            throw new ArgumentException("CheckOut must be after CheckIn");

        var tasks = _providers.Select(p => 
            p.GetAvailableRooms(query.Destination, query.CheckIn, query.CheckOut, query.RoomType)
        );

        var results = await Task.WhenAll(tasks);
        var allRooms = results.SelectMany(r => r).ToList();

        allRooms = allRooms.OrderBy(r => r.ProviderName).ThenBy(r => r.RoomType).ToList();

        return new SearchResponse
        {
            Rooms = allRooms,
            Destination = query.Destination,
            CheckIn = query.CheckIn,
            CheckOut = query.CheckOut
        };
    }
}
