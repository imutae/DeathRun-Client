public class SMovePacket
{
    private const int SESSION_ID_SIZE = 8;

    public long SessionId;
    public float X;
    public float Y;

    public static SMovePacket Deserialize(byte[] body)
    {
        SMovePacket packet = new SMovePacket();

        packet.SessionId = PacketSerializer.ReadInt64(body, 0);
        packet.X = PacketSerializer.ReadFloat(body, SESSION_ID_SIZE);
        packet.Y = PacketSerializer.ReadFloat(body, SESSION_ID_SIZE + 4);

        return packet;
    }
}