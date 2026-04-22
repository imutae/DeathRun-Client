public struct Packet
{
    public ushort Size;   // 전체 패킷 크기 (header 포함)
    public ushort Id;     // 패킷 ID
    public byte[] Body;   // 데이터
}
