using System.Runtime.InteropServices;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Structs;

public struct T3SamplerStateBlock
{
    public uint mData;

    public T3SamplerStateBlock(BinaryReader reader)
    {
        mData = reader.ReadUInt32(); //mSamplerState [4 bytes]
    }

    public readonly uint GetByteSize()
    {
        uint totalByteSize = 0;

        totalByteSize += (uint)Marshal.SizeOf(mData); //mData [4 bytes]

        return totalByteSize;
    }

    public override readonly string ToString()
    {
        string enumFlags = "";

        var allEnums = Enum.GetValues<T3SamplerStateValue>();

        foreach (var enumMask in allEnums)
        {
            if ((mData & (uint)(T3SamplerStateValue)enumMask) != 0)
            {
                string enumMakName = Enum.GetName((T3SamplerStateValue)enumMask) ?? throw new InvalidDataException("Hey, anyone there? We've got an unknown case! (null)");
                enumFlags += enumMakName[14..] + ": " + (mData & (uint)(T3SamplerStateValue)enumMask) + " | ";
            }
        }

        enumFlags += "(" + (int)mData + ")";
        return string.Format("[T3SamplerStateBlock] mData: {0}", enumFlags);
    }
}
