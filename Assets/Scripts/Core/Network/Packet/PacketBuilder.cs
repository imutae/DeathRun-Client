using System;

public static class PacketBuilder
{
    private const int HEADER_SIZE = 4;

    public static byte[] Build(ushort packetId, byte[] body)
    {
        ushort size = (ushort)(HEADER_SIZE + body.Length);

        byte[] packet = new byte[size];

        // size (little-endian)
        packet[0] = (byte)(size & 0xFF);
        packet[1] = (byte)((size >> 8) & 0xFF);

        // id
        packet[2] = (byte)(packetId & 0xFF);
        packet[3] = (byte)((packetId >> 8) & 0xFF);

        // body
        Buffer.BlockCopy(body, 0, packet, HEADER_SIZE, body.Length);

        return packet;
    }
}