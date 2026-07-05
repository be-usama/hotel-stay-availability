namespace HotelStay.Tests.TestDoubles;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelStay.Api.Models;
using HotelStay.Api.Providers;

/// <summary>
/// A controllable test double for IHotelProvider so service-level tests
/// don't depend on the real providers' pricing/business rules.
/// </summary>
public class FakeHotelProvider : IHotelProvider
{
    private readonly List<Room> _rooms;
    public bool WasCalled { get; private set; }

    public FakeHotelProvider(params Room[] rooms)
    {
        _rooms = rooms.ToList();
    }

    public Task<List<Room>> GetAvailableRooms(
        string destination,
        DateTime checkIn,
        DateTime checkOut,
        RoomType? roomType = null)
    {
        WasCalled = true;
        var results = roomType.HasValue
            ? _rooms.Where(r => r.RoomType == roomType.Value).ToList()
            : _rooms.ToList();

        return Task.FromResult(results);
    }
}
