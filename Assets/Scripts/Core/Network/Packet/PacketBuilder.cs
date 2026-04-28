using System;

public static class PacketBuilder
{
    public static byte[] Build(ushort packetId, byte[] body)
    {
        if (body == null)
        {
            body = new byte[0];
        }

        int packetSize = PacketRules.HeaderSize + body.Length;

        if (packetSize > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(body),
                "Packet size exceeds ushort range."
            );
        }

        if (packetSize > PacketRules.MaxPacketSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(body),
                $"Packet size exceeds max packet size. Max={PacketRules.MaxPacketSize}, Actual={packetSize}"
            );
        }

        byte[] packet = new byte[packetSize];

        ushort size = (ushort)packetSize;

        // size, little-endian
        packet[0] = (byte)(size & 0xFF);
        packet[1] = (byte)((size >> 8) & 0xFF);

        // id, little-endian
        packet[2] = (byte)(packetId & 0xFF);
        packet[3] = (byte)((packetId >> 8) & 0xFF);

        if (body.Length > 0)
        {
            Buffer.BlockCopy(body, 0, packet, PacketRules.HeaderSize, body.Length);
        }

        return packet;
    }
}