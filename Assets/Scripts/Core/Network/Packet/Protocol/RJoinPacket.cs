public class RJoinPacket
{
    public ushort RoomId;

    public byte[] Serialize()
    {
        byte[] body = new byte[2];

        PacketSerializer.WriteUInt16(body, 0, RoomId);

        return PacketBuilder.Build((ushort)PacketId.R_JOIN, body);
    }

    public static RJoinPacket Deserialize(byte[] body)
    {
        RJoinPacket packet = new RJoinPacket();
        packet.RoomId = PacketSerializer.ReadUInt16(body, 0);
        return packet;
    }
}