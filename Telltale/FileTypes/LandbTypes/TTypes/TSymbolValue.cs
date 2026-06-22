using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.TTypes;

[Description("Processing a SymbolValue(8 byte)")]
public class TSymbolValue : ReportBase, IParsable
{
    #region Private Fields

    #endregion

    #region Public Properties

    public ulong Value { get; set; }

    public uint Size => sizeof(ulong);

    public long Position { get; private set; }

    #endregion

    #region Public Methods

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        Value = reader.ReadUInt64();
        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;
        writer.Write(Value);
        Report(this, true);
    }

    #endregion
}