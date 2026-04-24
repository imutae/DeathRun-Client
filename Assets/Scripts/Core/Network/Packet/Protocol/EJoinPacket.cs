public class EJoinPacket
{
    public long SessionId;

    public static EJoinPacket Deserialize(byte[] body)
    {
        EJoinPacket packet = new EJoinPacket();
        packet.SessionId = PacketSerializer.ReadInt64(body, 0);
        return packet;
    }
}