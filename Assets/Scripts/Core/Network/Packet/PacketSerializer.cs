using System;
using System.Text;

public static class PacketSerializer
{
    /// <summary>
    /// 고정 길이 문자열 쓰기 (C++ char[] 대응)
    /// </summary>
    public static void WriteFixedString(byte[] buffer, int offset, int maxSize, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);

        int copySize = Math.Min(bytes.Length, maxSize);
        Buffer.BlockCopy(bytes, 0, buffer, offset, copySize);

        // 남는 부분은 자동으로 0 padding
    }

    /// <summary>
    /// 고정 길이 문자열 읽기
    /// </summary>
    public static string ReadFixedString(byte[] buffer, int offset, int maxSize)
    {
        int length = 0;

        // null 문자 기준 길이 찾기
        for (int i = 0; i < maxSize; i++)
        {
            if (buffer[offset + i] == 0)
                break;
            length++;
        }

        return Encoding.UTF8.GetString(buffer, offset, length);
    }
}