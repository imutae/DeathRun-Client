public class ELeavePacket
{
    public long SessionId;

    public static ELeavePacket Deserialize(byte[] body)
    {
        ELeavePacket packet = new ELeavePacket();
        packet.SessionId = PacketSerializer.ReadInt64(body, 0);
        return packet;
    }
}