namespace HotelStay.Api.Providers;

using HotelStay.Api.Models;

public interface IHotelProvider
{
    Task<List<Room>> GetAvailableRooms(
        string destination,
        DateTime checkIn,
        DateTime checkOut,
        RoomType? roomType = null);
}
