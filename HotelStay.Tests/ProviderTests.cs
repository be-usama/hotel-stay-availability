using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using HotelStay.Api.Models;
using HotelStay.Api.Providers;

namespace HotelStay.Tests;

public class ProviderTests
{
    [Fact]
    public async Task PremierStaysProvider_ReturnsRoomsForValidDestination()
    {
        var provider = new PremierStaysProvider();
        var checkIn = DateTime.Now.Date.AddDays(1);
        var checkOut = checkIn.AddDays(3);

        var rooms = await provider.GetAvailableRooms("Paris", checkIn, checkOut);

        Assert.NotEmpty(rooms);
        Assert.All(rooms, room =>
        {
            Assert.Equal("PremierStays", room.ProviderName);
            Assert.NotNull(room.StarRating);
            Assert.NotEmpty(room.Amenities);
        });
    }

    [Fact]
    public async Task BudgetNestsProvider_ReturnsRoomsForValidDestination()
    {
        var provider = new BudgetNestsProvider();
        var checkIn = DateTime.Now.Date.AddDays(1);
        var checkOut = checkIn.AddDays(3);

        var rooms = await provider.GetAvailableRooms("London", checkIn, checkOut);

        Assert.NotEmpty(rooms);
        Assert.All(rooms, room =>
        {
            Assert.Equal("BudgetNests", room.ProviderName);
        });
    }

    [Fact]
    public async Task Provider_CalculatesCorrectTotalPrice()
    {
        var provider = new PremierStaysProvider();
        var checkIn = DateTime.Now.Date.AddDays(1);
        var checkOut = checkIn.AddDays(4);

        var rooms = await provider.GetAvailableRooms("Paris", checkIn, checkOut, RoomType.Deluxe);

        var deluxeRoom = rooms.Single(r => r.RoomType == RoomType.Deluxe);
        Assert.Equal(150, deluxeRoom.PerNightRate); // Deluxe rate
        Assert.Equal(600, deluxeRoom.TotalStayPrice); // 4 nights * 150
        Assert.Equal(4, deluxeRoom.NumberOfNights);
    }

    [Fact]
    public async Task PremierStaysProvider_NoRoomTypeFilter_ReturnsAllRoomTypes()
    {
        var provider = new PremierStaysProvider();
        var checkIn = DateTime.Now.Date.AddDays(1);
        var checkOut = checkIn.AddDays(2);

        var rooms = await provider.GetAvailableRooms("Paris", checkIn, checkOut);

        Assert.Equal(3, rooms.Count);
        Assert.Contains(rooms, r => r.RoomType == RoomType.Standard);
        Assert.Contains(rooms, r => r.RoomType == RoomType.Deluxe);
        Assert.Contains(rooms, r => r.RoomType == RoomType.Suite);
    }

    [Fact]
    public async Task PremierStaysProvider_WithRoomTypeFilter_ReturnsOnlyThatType()
    {
        var provider = new PremierStaysProvider();
        var checkIn = DateTime.Now.Date.AddDays(1);
        var checkOut = checkIn.AddDays(2);

        var rooms = await provider.GetAvailableRooms("Paris", checkIn, checkOut, RoomType.Suite);

        var room = Assert.Single(rooms);
        Assert.Equal(RoomType.Suite, room.RoomType);
    }

    [Fact]
    public async Task BudgetNestsProvider_WithRoomTypeFilter_ReturnsOnlyThatType()
    {
        var provider = new BudgetNestsProvider();
        var checkIn = DateTime.Now.Date.AddDays(1);
        var checkOut = checkIn.AddDays(2);

        var rooms = await provider.GetAvailableRooms("London", checkIn, checkOut, RoomType.Standard);

        var room = Assert.Single(rooms);
        Assert.Equal(RoomType.Standard, room.RoomType);
        Assert.Equal(60, room.PerNightRate);
    }

    [Fact]
    public async Task BudgetNestsProvider_UsesFlexibleCancellationPolicy()
    {
        var provider = new BudgetNestsProvider();
        var checkIn = DateTime.Now.Date.AddDays(1);
        var checkOut = checkIn.AddDays(2);

        var rooms = await provider.GetAvailableRooms("London", checkIn, checkOut);

        Assert.All(rooms, room => Assert.Equal(CancellationPolicy.Flexible, room.CancellationPolicy));
    }

    [Fact]
    public async Task PremierStaysProvider_UsesFreeCancellationPolicy()
    {
        var provider = new PremierStaysProvider();
        var checkIn = DateTime.Now.Date.AddDays(1);
        var checkOut = checkIn.AddDays(2);

        var rooms = await provider.GetAvailableRooms("Paris", checkIn, checkOut);

        Assert.All(rooms, room => Assert.Equal(CancellationPolicy.FreeCancellation, room.CancellationPolicy));
    }
}
