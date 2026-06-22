namespace TelltaleLocalizationTool.Helper;

public static class ByteFunctions
{
    public static uint GetByteArrayListElementsCount(List<byte[]> array)
    {
        uint result = 0;

        foreach (byte[] t in array)
        {
            result += (uint)t.Length;
        }

        return result;
    }

    public static string ReadString(BinaryReader reader)
    {
        int stringLength = reader.ReadInt32();

        string value = "";

        for (int i = 0; i < stringLength; i++)
        {
            value += reader.ReadChar();
        }

        return value;
    }

    public static string ReadFixedString(BinaryReader reader, int length)
    {
        string value = "";

        for (int i = 0; i < length; i++)
        {
            value += reader.ReadChar();
        }

        return value;
    }

    public static bool ReadTelltaleBoolean(BinaryReader reader)
    {
        char parsedChar = reader.ReadChar();

        return parsedChar switch
        {
            '1' => true,
            '0' => false,
            _ => throw new Exception("Invalid Telltale Boolean data."),
        };
    }

    public static void WriteString(BinaryWriter writer, string value)
    {
        writer.Write(value.Length);

        foreach (char t in value)
        {
            writer.Write(t);
        }
    }

    public static void WriteFixedString(BinaryWriter writer, string value)
    {
        foreach (char t in value)
        {
            writer.Write(t);
        }
    }

    public static void WriteBoolean(BinaryWriter writer, bool value)
    {
        writer.Write(value ? '1' : '0');
    }

    public static byte[] Combine(byte[] first, byte[] second)
    {
        //allocate a byte array with both total lengths combined to accommodate both
        byte[] bytes = new byte[first.Length + second.Length];

        //copy the data from the first array into the new array
        Buffer.BlockCopy(first, 0, bytes, 0, first.Length);

        //copy the data from the second array into the new array (offset by the total length of the first array)
        Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);

        //return the final byte array
        return bytes;
    }
}
