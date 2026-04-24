public class RMovePacket
{
    public float X;
    public float Y;

    public byte[] Serialize()
    {
        byte[] body = new byte[8];

        PacketSerializer.WriteFloat(body, 0, X);
        PacketSerializer.WriteFloat(body, 4, Y);

        return PacketBuilder.Build((ushort)PacketId.R_MOVE, body);
    }

    public static RMovePacket Deserialize(byte[] body)
    {
        RMovePacket packet = new RMovePacket();

        packet.X = PacketSerializer.ReadFloat(body, 0);
        packet.Y = PacketSerializer.ReadFloat(body, 4);

        return packet;
    }
}