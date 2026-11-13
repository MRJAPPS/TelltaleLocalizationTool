using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

public interface ITelltaleObject : ISize, IWriteable, IPosition, IParsable
{
    public MetaStreamContext? msCtx { get; protected set; }
}
