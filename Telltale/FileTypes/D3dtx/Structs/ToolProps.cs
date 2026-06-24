using TelltaleLocalizationTool.Helper;

namespace TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Structs;

public struct ToolProps
{
    public bool mbHasProps;

    public ToolProps(BinaryReader reader)
    {
        mbHasProps = ByteFunctions.ReadTelltaleBoolean(reader);
    }

    public readonly uint GetByteSize()
    {
        uint totalByteSize = 0;

        totalByteSize += 1; //mbHasProps [1 bytes]

        return totalByteSize;
    }

    public override readonly string ToString() => string.Format("[ToolProps] mbHasProps: {0}", mbHasProps);
}
