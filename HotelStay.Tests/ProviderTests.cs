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

        Assert.InRange(rooms.Count, 0, 3);
        Assert.All(rooms, room =>
        {
            Assert.Equal("BudgetNests", room.ProviderName);
            Assert.Null(room.StarRating);
            Assert.Null(room.Amenities);
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
        Room? matchedRoom = null;

        for (var i = 0; i < 200; i++)
        {
            var rooms = await provider.GetAvailableRooms($"london-{i}", checkIn, checkOut, RoomType.Standard);
            if (rooms.Count == 1)
            {
                matchedRoom = rooms[0];
                break;
            }
        }

        Assert.NotNull(matchedRoom);
        Assert.Equal(RoomType.Standard, matchedRoom.RoomType);
        Assert.Equal(60, matchedRoom.PerNightRate);
    }

    [Fact]
    public async Task BudgetNestsProvider_UsesAllowedCancellationPolicies()
    {
        var provider = new BudgetNestsProvider();
        var checkIn = DateTime.Now.Date.AddDays(1);
        var checkOut = checkIn.AddDays(2);

        var rooms = await provider.GetAvailableRooms("London", checkIn, checkOut);

        Assert.All(
            rooms,
            room => Assert.Contains(
                room.CancellationPolicy,
                new[] { CancellationPolicy.Flexible, CancellationPolicy.NonRefundable }));
    }

    [Fact]
    public async Task PremierStaysProvider_UsesAllowedCancellationPolicies()
    {
        var provider = new PremierStaysProvider();
        var checkIn = DateTime.Now.Date.AddDays(1);
        var checkOut = checkIn.AddDays(2);

        var rooms = await provider.GetAvailableRooms("Paris", checkIn, checkOut);

        Assert.All(
            rooms,
            room => Assert.Contains(
                room.CancellationPolicy,
                new[] { CancellationPolicy.FreeCancellation, CancellationPolicy.NonRefundable }));
    }

    [Fact]
    public async Task BudgetNestsProvider_CanProduceUnavailableResultsThatGetFilteredOut()
    {
        var provider = new BudgetNestsProvider();
        var checkIn = DateTime.Now.Date.AddDays(1);
        var checkOut = checkIn.AddDays(2);

        var foundFilteredCase = false;
        for (var i = 0; i < 200; i++)
        {
            var rooms = await provider.GetAvailableRooms($"london-{i}", checkIn, checkOut, RoomType.Standard);
            if (rooms.Count == 0)
            {
                foundFilteredCase = true;
                break;
            }
        }

        Assert.True(foundFilteredCase, "Expected at least one deterministic unavailable BudgetNests room to be filtered.");
    }

    [Fact]
    public async Task PremierStaysProvider_CanProduceNonRefundablePolicy()
    {
        var provider = new PremierStaysProvider();
        var checkIn = DateTime.Now.Date.AddDays(1);
        var checkOut = checkIn.AddDays(2);

        var foundNonRefundable = false;
        for (var i = 0; i < 200; i++)
        {
            var rooms = await provider.GetAvailableRooms($"paris-{i}", checkIn, checkOut, RoomType.Standard);
            if (rooms.Single().CancellationPolicy == CancellationPolicy.NonRefundable)
            {
                foundNonRefundable = true;
                break;
            }
        }

        Assert.True(foundNonRefundable, "Expected deterministic PremierStays policy selection to produce NonRefundable.");
    }

    [Fact]
    public async Task BudgetNestsProvider_CanProduceNonRefundablePolicy()
    {
        var provider = new BudgetNestsProvider();
        var checkIn = DateTime.Now.Date.AddDays(1);
        var checkOut = checkIn.AddDays(2);

        var foundNonRefundable = false;
        for (var i = 0; i < 300; i++)
        {
            var rooms = await provider.GetAvailableRooms($"london-{i}", checkIn, checkOut, RoomType.Standard);
            if (rooms.Count == 1 && rooms[0].CancellationPolicy == CancellationPolicy.NonRefundable)
            {
                foundNonRefundable = true;
                break;
            }
        }

        Assert.True(foundNonRefundable, "Expected deterministic BudgetNests policy selection to produce NonRefundable.");
    }
}
