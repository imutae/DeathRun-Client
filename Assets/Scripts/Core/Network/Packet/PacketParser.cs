using System.Collections.Generic;

public class PacketParser
{
    private readonly List<byte> _buffer = new List<byte>();

    public void Append(byte[] data, int length)
    {
        if (data == null || length <= 0)
            return;

        int safeLength = length;

        if (safeLength > data.Length)
            safeLength = data.Length;

        for (int i = 0; i < safeLength; i++)
        {
            _buffer.Add(data[i]);
        }
    }

    public bool TryGetPacket(out Packet packet)
    {
        packet = default;

        if (_buffer.Count < PacketRules.HeaderSize)
            return false;

        ushort size = (ushort)(_buffer[0] | (_buffer[1] << 8));

        if (!PacketRules.IsValidPacketSize(size))
        {
            Clear();
            return false;
        }

        if (_buffer.Count < size)
            return false;

        ushort id = (ushort)(_buffer[2] | (_buffer[3] << 8));

        int bodySize = size - PacketRules.HeaderSize;
        byte[] body = new byte[bodySize];

        if (bodySize > 0)
        {
            _buffer.CopyTo(PacketRules.HeaderSize, body, 0, bodySize);
        }

        _buffer.RemoveRange(0, size);

        packet = new Packet
        {
            Size = size,
            Id = id,
            Body = body
        };

        return true;
    }

    public void Clear()
    {
        _buffer.Clear();
    }
}