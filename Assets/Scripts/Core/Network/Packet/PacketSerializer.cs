using System;
using System.Text;

public static class PacketSerializer
{
    public static void WriteInt64(byte[] buffer, int offset, long value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
    }

    public static long ReadInt64(byte[] buffer, int offset)
    {
        return BitConverter.ToInt64(buffer, offset);
    }

    public static void WriteUInt16(byte[] buffer, int offset, ushort value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
    }

    public static ushort ReadUInt16(byte[] buffer, int offset)
    {
        return BitConverter.ToUInt16(buffer, offset);
    }

    public static void WriteByte(byte[] buffer, int offset, byte value)
    {
        buffer[offset] = value;
    }

    public static byte ReadByte(byte[] buffer, int offset)
    {
        return buffer[offset];
    }

    public static void WriteFloat(byte[] buffer, int offset, float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
    }

    public static float ReadFloat(byte[] buffer, int offset)
    {
        return BitConverter.ToSingle(buffer, offset);
    }

    /// <summary>
    /// 고정 길이 문자열 쓰기 (C++ char[] 대응)
    /// </summary>
    public static void WriteFixedString(byte[] buffer, int offset, int maxSize, string value)
    {
        Array.Clear(buffer, offset, maxSize);

        if (string.IsNullOrEmpty(value))
            return;

        byte[] bytes = Encoding.UTF8.GetBytes(value);
        int copySize = Math.Min(bytes.Length, maxSize);

        Buffer.BlockCopy(bytes, 0, buffer, offset, copySize);
    }

    /// <summary>
    /// 고정 길이 문자열 읽기
    /// </summary>
    public static string ReadFixedString(byte[] buffer, int offset, int maxSize)
    {
        int length = 0;

        for (int i = 0; i < maxSize; i++)
        {
            if (buffer[offset + i] == 0)
                break;

            length++;
        }

        return Encoding.UTF8.GetString(buffer, offset, length);
    }
}