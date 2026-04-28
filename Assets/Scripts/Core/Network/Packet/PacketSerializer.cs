using System;
using System.Text;

public static class PacketSerializer
{
    public static void WriteInt64(byte[] buffer, int offset, long value)
    {
        ValidateRange(buffer, offset, sizeof(long));

        byte[] bytes = BitConverter.GetBytes(value);
        EnsureLittleEndian(bytes);

        Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
    }

    public static long ReadInt64(byte[] buffer, int offset)
    {
        ValidateRange(buffer, offset, sizeof(long));

        byte[] bytes = new byte[sizeof(long)];
        Buffer.BlockCopy(buffer, offset, bytes, 0, bytes.Length);

        EnsureLittleEndian(bytes);

        return BitConverter.ToInt64(bytes, 0);
    }

    public static void WriteUInt16(byte[] buffer, int offset, ushort value)
    {
        ValidateRange(buffer, offset, sizeof(ushort));

        byte[] bytes = BitConverter.GetBytes(value);
        EnsureLittleEndian(bytes);

        Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
    }

    public static ushort ReadUInt16(byte[] buffer, int offset)
    {
        ValidateRange(buffer, offset, sizeof(ushort));

        byte[] bytes = new byte[sizeof(ushort)];
        Buffer.BlockCopy(buffer, offset, bytes, 0, bytes.Length);

        EnsureLittleEndian(bytes);

        return BitConverter.ToUInt16(bytes, 0);
    }

    public static void WriteByte(byte[] buffer, int offset, byte value)
    {
        ValidateRange(buffer, offset, sizeof(byte));

        buffer[offset] = value;
    }

    public static byte ReadByte(byte[] buffer, int offset)
    {
        ValidateRange(buffer, offset, sizeof(byte));

        return buffer[offset];
    }

    public static void WriteFloat(byte[] buffer, int offset, float value)
    {
        ValidateRange(buffer, offset, sizeof(float));

        byte[] bytes = BitConverter.GetBytes(value);
        EnsureLittleEndian(bytes);

        Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
    }

    public static float ReadFloat(byte[] buffer, int offset)
    {
        ValidateRange(buffer, offset, sizeof(float));

        byte[] bytes = new byte[sizeof(float)];
        Buffer.BlockCopy(buffer, offset, bytes, 0, bytes.Length);

        EnsureLittleEndian(bytes);

        return BitConverter.ToSingle(bytes, 0);
    }

    public static void WriteFixedString(byte[] buffer, int offset, int maxSize, string value)
    {
        ValidateRange(buffer, offset, maxSize);

        Array.Clear(buffer, offset, maxSize);

        if (string.IsNullOrEmpty(value))
            return;

        byte[] bytes = Encoding.UTF8.GetBytes(value);
        int copySize = Math.Min(bytes.Length, maxSize);

        Buffer.BlockCopy(bytes, 0, buffer, offset, copySize);
    }

    public static string ReadFixedString(byte[] buffer, int offset, int maxSize)
    {
        ValidateStringReadRange(buffer, offset, maxSize);

        int readableSize = Math.Min(maxSize, buffer.Length - offset);
        int length = 0;

        for (int i = 0; i < readableSize; i++)
        {
            if (buffer[offset + i] == 0)
                break;

            length++;
        }

        if (length == 0)
            return string.Empty;

        return Encoding.UTF8.GetString(buffer, offset, length);
    }

    private static void ValidateRange(byte[] buffer, int offset, int size)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset));

        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));

        if (offset + size > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                $"Buffer range is invalid. BufferLength={buffer.Length}, Offset={offset}, Size={size}"
            );
        }
    }

    private static void ValidateStringReadRange(byte[] buffer, int offset, int maxSize)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset));

        if (maxSize < 0)
            throw new ArgumentOutOfRangeException(nameof(maxSize));

        if (offset > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                $"Offset exceeds buffer length. BufferLength={buffer.Length}, Offset={offset}"
            );
        }
    }

    private static void EnsureLittleEndian(byte[] bytes)
    {
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
    }
}