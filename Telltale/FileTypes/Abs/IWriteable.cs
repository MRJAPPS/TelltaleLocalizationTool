using System.Xml;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

public interface IWriteable : IPosition
{
    #region Methods

    void Write(BinaryWriter writer);

    void Write(XmlElement element, string name);

    #endregion
}