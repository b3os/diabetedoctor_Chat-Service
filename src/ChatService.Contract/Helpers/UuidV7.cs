using System.Security.Cryptography;

namespace ChatService.Contract.Helpers;

public readonly struct UuidV7
{
    public readonly Guid Value;

    public UuidV7() : this(DateTimeOffset.UtcNow) { }

    private UuidV7(DateTimeOffset dateTimeOffset) {
        // bytes [0-5]: datetimeoffset yyyy-MM-dd hh:mm:ss fff
        // bytes [6]: 4 bits dedicated to guid version (version: 7)
        // bytes [6]: 4 bits dedicated to random part
        // bytes [7-15]: random part
        var uuidAsBytes = new byte[16];
        FillTimePart(ref uuidAsBytes, dateTimeOffset);
        var randomPart = uuidAsBytes.AsSpan().Slice(6);
        RandomNumberGenerator.Fill(randomPart);
        // add mask to set guid version
        uuidAsBytes[6] &= 0x0F;
        uuidAsBytes[6] += 0x70;
        Value = new Guid(uuidAsBytes, true);
    }

    private static void FillTimePart(ref byte[] uuidAsBytes, DateTimeOffset dateTimeOffset) {
        var currentTimestamp = dateTimeOffset.ToUnixTimeMilliseconds();
        var current = BitConverter.GetBytes(currentTimestamp);
        if (BitConverter.IsLittleEndian) {
            Array.Reverse(current);
        }
        current[2..8].CopyTo(uuidAsBytes, 0);
    }
    
    public DateTimeOffset GetDateTimeOffset() 
    {
        var bytes = new byte[8];
        Value.ToByteArray(true)[..6].CopyTo(bytes, 2);
        if (BitConverter.IsLittleEndian) 
        {
            Array.Reverse(bytes);
        }
        var ms = BitConverter.ToInt64(bytes);
        return DateTimeOffset.FromUnixTimeMilliseconds(ms);
    }
}