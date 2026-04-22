using System;
using System.Collections.Generic;

public class PacketParser
{
    private List<byte> _buffer = new List<byte>();
    private const int HEADER_SIZE = 4;

    public void Append(byte[] data, int length)
    {
        for (int i = 0; i < length; i++)
            _buffer.Add(data[i]);
    }

    public bool TryGetPacket(out Packet packet)
    {
        packet = default;

        if (_buffer.Count < HEADER_SIZE)
            return false;

        ushort size = (ushort)(_buffer[0] | (_buffer[1] << 8));

        if (_buffer.Count < size)
            return false;

        ushort id = (ushort)(_buffer[2] | (_buffer[3] << 8));

        int bodySize = size - HEADER_SIZE;
        byte[] body = new byte[bodySize];

        if (bodySize > 0)
        {
            _buffer.CopyTo(HEADER_SIZE, body, 0, bodySize);
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