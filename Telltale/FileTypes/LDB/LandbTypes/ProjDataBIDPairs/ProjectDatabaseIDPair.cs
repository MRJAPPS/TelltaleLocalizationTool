using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;

namespace TelltaleLocalizationTool.Telltale.FileTypes.LDB.LandbTypes.ProjDataBIDPairs;

[Description("Processing a collection of ProjectDatabase that contains mProjectID and mDBID.")]
public class ProjectDatabaseIDPair : ReportBase, ITelltaleObject
{
    #region Fields


    #endregion

    #region Properties

    private uint mProjectID { get; set; }

    private uint mDBID { get; set; }

    public uint Size => sizeof(uint) * 2;

    public long Position { get; private set; }

    #endregion

    #region Implement

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;

        mProjectID = reader.ReadUInt32();
        mDBID = reader.ReadUInt32();

        Report(this);
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;

        writer.Write(mProjectID);
        writer.Write(mDBID);

        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(ProjectDatabaseIDPair));

        myElement.SetAttribute(nameof(name), name);

        AppendBasicTypes(myElement);

        _ = element.AppendChild(myElement);
    }

    #endregion

    #region XML

    private void AppendBasicTypes(XmlElement element)
    {
        XmlAttribute mProjectID, mDBID;

        mProjectID = element.OwnerDocument.CreateAttribute(nameof(this.mProjectID));
        mDBID = element.OwnerDocument.CreateAttribute(nameof(this.mDBID));

        mProjectID.Value = this.mProjectID.ToString();
        mDBID.Value = this.mDBID.ToString();

        _ = element.Attributes.Append(mProjectID);
        _ = element.Attributes.Append(mDBID);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(ProjectDatabaseIDPair), name);
        mProjectID = uint.Parse( myElement.GetAttribute(nameof(mProjectID)));
        mDBID = uint.Parse( myElement.GetAttribute(nameof(mDBID)));
    }

    #endregion
}
