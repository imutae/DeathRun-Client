using System;

public class SJoinPacket
{
    private const int SUCCESS_SIZE = 1;
    private const int PLAYER_COUNT_SIZE = 1;
    private const int HEADER_SIZE = SUCCESS_SIZE + PLAYER_COUNT_SIZE;

    private const int SUCCESS_OFFSET = 0;
    private const int PLAYER_COUNT_OFFSET = 1;
    private const int SESSION_IDS_OFFSET = 2;

    private const int SESSION_ID_SIZE = 8;

    public byte Success;
    public byte PlayerCount;
    public long[] SessionIds = new long[ProtocolConstants.MaxRoomPlayers];

    public bool IsSuccess => Success != 0;

    public static SJoinPacket Deserialize(byte[] body)
    {
        if (body == null || body.Length < HEADER_SIZE)
        {
            throw new ArgumentException("Invalid SJoinPacket body.", nameof(body));
        }

        SJoinPacket packet = new SJoinPacket();

        packet.Success = PacketSerializer.ReadByte(body, SUCCESS_OFFSET);
        packet.PlayerCount = PacketSerializer.ReadByte(body, PLAYER_COUNT_OFFSET);

        int availablePlayerCount = (body.Length - SESSION_IDS_OFFSET) / SESSION_ID_SIZE;
        int playerCount = Math.Min(
            packet.PlayerCount,
            Math.Min(availablePlayerCount, ProtocolConstants.MaxRoomPlayers)
        );

        packet.PlayerCount = (byte)playerCount;

        int offset = SESSION_IDS_OFFSET;

        for (int i = 0; i < playerCount; i++)
        {
            packet.SessionIds[i] = PacketSerializer.ReadInt64(body, offset);
            offset += SESSION_ID_SIZE;
        }

        return packet;
    }
}