namespace HotelStay.Api.Providers;

using HotelStay.Api.Models;

public class PremierStaysProvider : IHotelProvider
{
    public Task<List<Room>> GetAvailableRooms(
        string destination,
        DateTime checkIn,
        DateTime checkOut,
        RoomType? roomType = null)
    {
        var numberOfNights = (checkOut.Date - checkIn.Date).Days;
        var roomTypes = roomType.HasValue 
            ? new[] { roomType.Value } 
            : new[] { RoomType.Standard, RoomType.Deluxe, RoomType.Suite };

        var rooms = roomTypes
            .Select(type => new PremierStaysRoomResponse
            {
                RoomType = type,
                PerNightRate = GetPerNightRate(type),
                CancellationPolicy = SelectCancellationPolicy(destination, type, checkIn),
                StarRating = "5",
                Amenities = new List<string> { "WiFi", "Pool", "Gym", "Spa" }
            })
            .Select(raw => new Room
            {
                RoomId = $"ps-{destination.ToLowerInvariant()}-{raw.RoomType.ToString().ToLowerInvariant()}",
                ProviderName = "PremierStays",
                RoomType = raw.RoomType,
                PerNightRate = raw.PerNightRate,
                TotalStayPrice = raw.PerNightRate * numberOfNights,
                CancellationPolicy = raw.CancellationPolicy,
                StarRating = raw.StarRating,
                Amenities = raw.Amenities,
                NumberOfNights = numberOfNights
            })
            .ToList();

        return Task.FromResult(rooms);
    }

    private static decimal GetPerNightRate(RoomType type) =>
        type switch
        {
            RoomType.Standard => 100m,
            RoomType.Deluxe => 150m,
            RoomType.Suite => 250m,
            _ => 100m
        };

    private static CancellationPolicy SelectCancellationPolicy(string destination, RoomType roomType, DateTime checkIn)
    {
        var bucket = ProviderDeterminism.GetBucket("PremierStays", destination, roomType, checkIn, 2);
        return bucket == 0
            ? CancellationPolicy.FreeCancellation
            : CancellationPolicy.NonRefundable;
    }

    private sealed class PremierStaysRoomResponse
    {
        public RoomType RoomType { get; set; }
        public decimal PerNightRate { get; set; }
        public CancellationPolicy CancellationPolicy { get; set; }
        public string StarRating { get; set; } = string.Empty;
        public List<string> Amenities { get; set; } = new();
    }
}
