namespace HotelStay.Api.Providers;

using System.Security.Cryptography;
using System.Text;
using HotelStay.Api.Models;

internal static class ProviderDeterminism
{
    public static int GetBucket(
        string providerName,
        string destination,
        RoomType roomType,
        DateTime checkIn,
        int modulo)
    {
        if (modulo <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(modulo), "Modulo must be positive.");
        }

        var normalizedDestination = destination.Trim().ToLowerInvariant();
        var key = $"{providerName}|{normalizedDestination}|{roomType}|{checkIn:yyyy-MM-dd}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var numeric = BitConverter.ToUInt32(hash, 0);

        return (int)(numeric % (uint)modulo);
    }
}
