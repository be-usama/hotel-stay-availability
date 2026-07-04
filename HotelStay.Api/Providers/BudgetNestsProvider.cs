namespace HotelStay.Api.Providers;

using HotelStay.Api.Models;

public class BudgetNestsProvider : IHotelProvider
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
                RoomType.Standard => 60m,
                RoomType.Deluxe => 90m,
                RoomType.Suite => 150m,
                _ => 60m
            };

            rooms.Add(new Room
            {
                RoomId = $"bn-{destination.ToLower()}-{type.ToString().ToLower()}",
                ProviderName = "BudgetNests",
                RoomType = type,
                PerNightRate = perNightRate,
                TotalStayPrice = perNightRate * numberOfNights,
                CancellationPolicy = CancellationPolicy.Flexible,
                NumberOfNights = numberOfNights
            });
        }

        return Task.FromResult(rooms);
    }
}
