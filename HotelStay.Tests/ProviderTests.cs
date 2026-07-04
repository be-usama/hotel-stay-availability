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
}
