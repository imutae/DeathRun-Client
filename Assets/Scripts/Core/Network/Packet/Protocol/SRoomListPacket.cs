public class SRoomListPacket
{
    private const int ROOM_INFO_SIZE = 3;

    public byte RoomCount;
    public RoomInfo[] Rooms = new RoomInfo[ProtocolConstants.MaxRoomCount];

    public static SRoomListPacket Deserialize(byte[] body)
    {
        SRoomListPacket packet = new SRoomListPacket();

        packet.RoomCount = PacketSerializer.ReadByte(body, 0);

        int offset = 1;
        for (int i = 0; i < ProtocolConstants.MaxRoomCount; i++)
        {
            RoomInfo roomInfo = new RoomInfo();
            roomInfo.RoomId = PacketSerializer.ReadUInt16(body, offset);
            roomInfo.CurrentPlayers = PacketSerializer.ReadByte(body, offset + 2);

            packet.Rooms[i] = roomInfo;
            offset += ROOM_INFO_SIZE;
        }

        return packet;
    }
}