using System.ComponentModel;
using System.Reflection.PortableExecutable;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.TTypes;

[Description("Processing THandle(a wrapper for TSymbolValue + 4 byte extra)")]
public class THandle : ReportBase, IParsable
{
    #region Private Fields

    #endregion

    #region Public Properties

    public TSymbolValue hSymbolValue { get; set; } = new();

    public uint Size => hSymbolValue.Size + ISize.FOUR_ADD;

    public long Position { get; private set; }

    #endregion

    #region Public Methods

    public void Parse(BinaryReader reader)
    {
        // Stash the start pos for the report – gotta flex that offset later.
        Position = reader.BaseStream.Position;

        // Those first 4 bytes are just a duplicate size we can figure out ourselves. Nom them and move on.
        _ = reader.ReadUInt32();

        hSymbolValue.Parse(reader);
        
        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;
        writer.Write(Size);

        hSymbolValue.Write(writer);

        Report(this, true);
    }

    #endregion
}