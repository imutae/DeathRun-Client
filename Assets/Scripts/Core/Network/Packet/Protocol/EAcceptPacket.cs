using System;

public class EAcceptPacket
{
    public long SessionId;

    public static EAcceptPacket Deserialize(byte[] body)
    {
        EAcceptPacket packet = new EAcceptPacket();

        packet.SessionId = PacketSerializer.ReadInt64(body, 0);

        return packet;
    }
}