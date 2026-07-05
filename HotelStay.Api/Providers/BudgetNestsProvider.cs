namespace HotelStay.Api.Providers;

using HotelStay.Api.Models;
using System.Text.Json.Serialization;

public class BudgetNestsProvider : IHotelProvider
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
            .Select(type => new BudgetNestsRoomResponse
            {
                RoomType = type,
                PerNightRate = GetPerNightRate(type),
                CancellationPolicy = SelectCancellationPolicy(destination, type, checkIn),
                Available = IsAvailable(destination, type, checkIn)
            })
            .Where(raw => raw.Available)
            .Select(raw => new Room
            {
                RoomId = $"bn-{destination.ToLowerInvariant()}-{raw.RoomType.ToString().ToLowerInvariant()}",
                ProviderName = "BudgetNests",
                RoomType = raw.RoomType,
                PerNightRate = raw.PerNightRate,
                TotalStayPrice = raw.PerNightRate * numberOfNights,
                CancellationPolicy = raw.CancellationPolicy,
                NumberOfNights = numberOfNights
            })
            .ToList();

        return Task.FromResult(rooms);
    }

    private static decimal GetPerNightRate(RoomType type) =>
        type switch
        {
            RoomType.Standard => 60m,
            RoomType.Deluxe => 90m,
            RoomType.Suite => 150m,
            _ => 60m
        };

    private static CancellationPolicy SelectCancellationPolicy(string destination, RoomType roomType, DateTime checkIn)
    {
        var bucket = ProviderDeterminism.GetBucket("BudgetNests-Policy", destination, roomType, checkIn, 2);
        return bucket == 0
            ? CancellationPolicy.Flexible
            : CancellationPolicy.NonRefundable;
    }

    private static bool IsAvailable(string destination, RoomType roomType, DateTime checkIn)
    {
        var bucket = ProviderDeterminism.GetBucket("BudgetNests-Availability", destination, roomType, checkIn, 4);
        return bucket != 0;
    }

    private sealed class BudgetNestsRoomResponse
    {
        [JsonPropertyName("room_type")]
        public RoomType RoomType { get; set; }

        [JsonPropertyName("per_night_rate")]
        public decimal PerNightRate { get; set; }

        [JsonPropertyName("cancellation_policy")]
        public CancellationPolicy CancellationPolicy { get; set; }

        [JsonPropertyName("available")]
        public bool Available { get; set; }
    }
}
