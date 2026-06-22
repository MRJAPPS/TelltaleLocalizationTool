using System.Xml;

namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

public interface IParsable
{

    #region Methods

    void Parse(BinaryReader reader);

    void Parse(XmlElement reader, string name);

    #endregion
}