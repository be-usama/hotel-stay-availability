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
        var rooms = new List<Room>();

        var roomTypes = roomType.HasValue 
            ? new[] { roomType.Value } 
            : new[] { RoomType.Standard, RoomType.Deluxe, RoomType.Suite };

        foreach (var type in roomTypes)
        {
            var perNightRate = type switch
            {
                RoomType.Standard => 100m,
                RoomType.Deluxe => 150m,
                RoomType.Suite => 250m,
                _ => 100m
            };

            rooms.Add(new Room
            {
                RoomId = $"ps-{destination.ToLower()}-{type.ToString().ToLower()}",
                ProviderName = "PremierStays",
                RoomType = type,
                PerNightRate = perNightRate,
                TotalStayPrice = perNightRate * numberOfNights,
                CancellationPolicy = CancellationPolicy.FreeCancellation,
                StarRating = "5",
                Amenities = new List<string> { "WiFi", "Pool", "Gym", "Spa" },
                NumberOfNights = numberOfNights
            });
        }

        return Task.FromResult(rooms);
    }
}
