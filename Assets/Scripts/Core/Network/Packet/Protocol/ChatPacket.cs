public class ChatPacket
{
    private const int SESSION_ID_SIZE = 8;
    private const int MESSAGE_SIZE = ProtocolConstants.MaxChatLength;

    public long SessionId;
    public string Message = string.Empty;

    public byte[] Serialize()
    {
        byte[] body = new byte[SESSION_ID_SIZE + MESSAGE_SIZE];

        PacketSerializer.WriteInt64(body, 0, SessionId);
        PacketSerializer.WriteFixedString(body, SESSION_ID_SIZE, MESSAGE_SIZE, Message);

        return PacketBuilder.Build((ushort)PacketId.R_CHAT, body);
    }

    public static ChatPacket Deserialize(byte[] body)
    {
        ChatPacket packet = new ChatPacket();

        packet.SessionId = PacketSerializer.ReadInt64(body, 0);
        packet.Message = PacketSerializer.ReadFixedString(body, SESSION_ID_SIZE, MESSAGE_SIZE);

        return packet;
    }
}