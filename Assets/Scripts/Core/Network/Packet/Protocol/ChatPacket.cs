public class ChatPacket
{
    public long UserName;
    public string Message;

    private const int USERNAME_SIZE = 32;
    private const int MESSAGE_SIZE = 256;

    public byte[] Serialize()
    {
        byte[] body = new byte[USERNAME_SIZE + MESSAGE_SIZE];

        PacketSerializer.WriteInt64(body, 0, UserName);
        PacketSerializer.WriteFixedString(body, USERNAME_SIZE, MESSAGE_SIZE, Message);

        return PacketBuilder.Build(1, body);
    }

    public static ChatPacket Deserialize(byte[] body)
    {
        ChatPacket packet = new ChatPacket();

        packet.UserName = PacketSerializer.ReadInt64(body, 0);
        packet.Message = PacketSerializer.ReadFixedString(body, USERNAME_SIZE, MESSAGE_SIZE);

        return packet;
    }
}