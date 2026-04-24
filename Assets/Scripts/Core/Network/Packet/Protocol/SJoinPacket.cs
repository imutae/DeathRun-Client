public class SJoinPacket
{
    private const int SESSION_ID_SIZE = 8;

    public byte Success;
    public byte PlayerCount;
    public long[] SessionIds = new long[ProtocolConstants.MaxRoomPlayers];

    public bool IsSuccess => Success != 0;

    public static SJoinPacket Deserialize(byte[] body)
    {
        SJoinPacket packet = new SJoinPacket();

        packet.Success = PacketSerializer.ReadByte(body, 0);
        packet.PlayerCount = PacketSerializer.ReadByte(body, 1);

        int offset = 2;
        for (int i = 0; i < ProtocolConstants.MaxRoomPlayers; i++)
        {
            packet.SessionIds[i] = PacketSerializer.ReadInt64(body, offset);
            offset += SESSION_ID_SIZE;
        }

        return packet;
    }
}