public class NLeavePacket
{
    public byte Reserved = 0;

    public byte[] Serialize()
    {
        byte[] body = new byte[1];

        PacketSerializer.WriteByte(body, 0, Reserved);

        return PacketBuilder.Build((ushort)PacketId.N_LEAVE, body);
    }
}