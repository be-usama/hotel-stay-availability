using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using HotelStay.Api.Models;
using HotelStay.Api.Services;
using HotelStay.Tests.TestDoubles;

namespace HotelStay.Tests;

public class HotelSearchServiceTests
{
    private static Room MakeRoom(string providerName, RoomType type, decimal rate) => new()
    {
        RoomId = $"{providerName.ToLower()}-{type.ToString().ToLower()}",
        ProviderName = providerName,
        RoomType = type,
        PerNightRate = rate,
        TotalStayPrice = rate * 2,
        CancellationPolicy = CancellationPolicy.Flexible,
        NumberOfNights = 2
    };

    [Fact]
    public async Task SearchHotels_AggregatesRoomsFromAllProviders()
    {
        var providerA = new FakeHotelProvider(MakeRoom("Alpha", RoomType.Standard, 100m));
        var providerB = new FakeHotelProvider(MakeRoom("Beta", RoomType.Deluxe, 200m));
        var service = new HotelSearchService(providerA, providerB);

        var result = await service.SearchHotels(new HotelSearchQuery
        {
            Destination = "Paris",
            CheckIn = DateTime.Today.AddDays(1),
            CheckOut = DateTime.Today.AddDays(3)
        });

        Assert.Equal(2, result.Rooms.Count);
        Assert.Contains(result.Rooms, r => r.ProviderName == "Alpha");
        Assert.Contains(result.Rooms, r => r.ProviderName == "Beta");
        Assert.True(providerA.WasCalled);
        Assert.True(providerB.WasCalled);
    }

    [Fact]
    public async Task SearchHotels_SortsResultsByProviderThenRoomType()
    {
        var providerA = new FakeHotelProvider(
            MakeRoom("Zulu", RoomType.Suite, 300m),
            MakeRoom("Zulu", RoomType.Standard, 100m));
        var providerB = new FakeHotelProvider(MakeRoom("Alpha", RoomType.Deluxe, 200m));
        var service = new HotelSearchService(providerA, providerB);

        var result = await service.SearchHotels(new HotelSearchQuery
        {
            Destination = "Paris",
            CheckIn = DateTime.Today.AddDays(1),
            CheckOut = DateTime.Today.AddDays(3)
        });

        Assert.Equal("Alpha", result.Rooms[0].ProviderName);
        Assert.Equal("Zulu", result.Rooms[1].ProviderName);
        Assert.Equal(RoomType.Standard, result.Rooms[1].RoomType);
        Assert.Equal("Zulu", result.Rooms[2].ProviderName);
        Assert.Equal(RoomType.Suite, result.Rooms[2].RoomType);
    }

    [Fact]
    public async Task SearchHotels_ReturnsEchoedQueryDetails()
    {
        var service = new HotelSearchService(new FakeHotelProvider());
        var checkIn = DateTime.Today.AddDays(1);
        var checkOut = DateTime.Today.AddDays(5);

        var result = await service.SearchHotels(new HotelSearchQuery
        {
            Destination = "Tokyo",
            CheckIn = checkIn,
            CheckOut = checkOut
        });

        Assert.Equal("Tokyo", result.Destination);
        Assert.Equal(checkIn, result.CheckIn);
        Assert.Equal(checkOut, result.CheckOut);
        Assert.Empty(result.Rooms);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchHotels_MissingDestination_ThrowsArgumentException(string? destination)
    {
        var service = new HotelSearchService(new FakeHotelProvider());

        var query = new HotelSearchQuery
        {
            Destination = destination!,
            CheckIn = DateTime.Today.AddDays(1),
            CheckOut = DateTime.Today.AddDays(2)
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.SearchHotels(query));
        Assert.Contains("Destination is required", ex.Message);
    }

    [Fact]
    public async Task SearchHotels_CheckOutEqualsCheckIn_ThrowsArgumentException()
    {
        var service = new HotelSearchService(new FakeHotelProvider());
        var sameDay = DateTime.Today.AddDays(1);

        var query = new HotelSearchQuery
        {
            Destination = "Paris",
            CheckIn = sameDay,
            CheckOut = sameDay
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.SearchHotels(query));
        Assert.Contains("CheckOut must be after CheckIn", ex.Message);
    }

    [Fact]
    public async Task SearchHotels_CheckOutBeforeCheckIn_ThrowsArgumentException()
    {
        var service = new HotelSearchService(new FakeHotelProvider());

        var query = new HotelSearchQuery
        {
            Destination = "Paris",
            CheckIn = DateTime.Today.AddDays(5),
            CheckOut = DateTime.Today.AddDays(1)
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.SearchHotels(query));
    }

    [Fact]
    public async Task SearchHotels_NoProviders_ReturnsEmptyRoomList()
    {
        var service = new HotelSearchService();

        var result = await service.SearchHotels(new HotelSearchQuery
        {
            Destination = "Paris",
            CheckIn = DateTime.Today.AddDays(1),
            CheckOut = DateTime.Today.AddDays(2)
        });

        Assert.Empty(result.Rooms);
    }

    [Fact]
    public async Task SearchHotels_FiltersByRoomTypeAcrossProviders()
    {
        var providerA = new FakeHotelProvider(
            MakeRoom("Alpha", RoomType.Standard, 100m),
            MakeRoom("Alpha", RoomType.Suite, 300m));
        var service = new HotelSearchService(providerA);

        var result = await service.SearchHotels(new HotelSearchQuery
        {
            Destination = "Paris",
            CheckIn = DateTime.Today.AddDays(1),
            CheckOut = DateTime.Today.AddDays(2),
            RoomType = RoomType.Suite
        });

        var room = Assert.Single(result.Rooms);
        Assert.Equal(RoomType.Suite, room.RoomType);
    }
}
