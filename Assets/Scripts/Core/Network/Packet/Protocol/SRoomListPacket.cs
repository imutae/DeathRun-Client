using System;

public class SRoomListPacket
{
    private const int ROOM_COUNT_SIZE = 1;
    private const int ROOM_INFO_SIZE = 3;

    private const int ROOM_ID_OFFSET = 0;
    private const int CURRENT_PLAYERS_OFFSET = 2;

    public byte RoomCount;
    public RoomInfo[] Rooms = new RoomInfo[ProtocolConstants.MaxRoomCount];

    public static SRoomListPacket Deserialize(byte[] body)
    {
        if (body == null || body.Length < ROOM_COUNT_SIZE)
        {
            throw new ArgumentException("Invalid SRoomListPacket body.", nameof(body));
        }

        SRoomListPacket packet = new SRoomListPacket();

        byte declaredRoomCount = PacketSerializer.ReadByte(body, 0);

        int availableRoomCount = (body.Length - ROOM_COUNT_SIZE) / ROOM_INFO_SIZE;
        int roomCount = Math.Min(
            declaredRoomCount,
            Math.Min(availableRoomCount, ProtocolConstants.MaxRoomCount)
        );

        packet.RoomCount = (byte)roomCount;

        int offset = ROOM_COUNT_SIZE;

        for (int i = 0; i < roomCount; i++)
        {
            RoomInfo roomInfo = new RoomInfo();

            roomInfo.RoomId = PacketSerializer.ReadUInt16(body, offset + ROOM_ID_OFFSET);
            roomInfo.CurrentPlayers = PacketSerializer.ReadByte(body, offset + CURRENT_PLAYERS_OFFSET);

            packet.Rooms[i] = roomInfo;

            offset += ROOM_INFO_SIZE;
        }

        return packet;
    }
}