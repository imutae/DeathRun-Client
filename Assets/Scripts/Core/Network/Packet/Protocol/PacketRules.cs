public static class PacketRules
{
    public const int HeaderSize = 4;

    // 현재 프로토콜 크기는 작지만, 비정상 패킷으로 버퍼가 과도하게 커지는 것을 막기 위한 제한값.
    // 서버와 최대 패킷 크기를 명확히 합의하면 그 값으로 조정하면 됨.
    public const int MaxPacketSize = 4096;

    public static bool IsValidPacketSize(int size)
    {
        return size >= HeaderSize && size <= MaxPacketSize;
    }
}